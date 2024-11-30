// 
// Copyright (c) 2024-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;

namespace FramePFX.Avalonia.Utils;

/// <summary>
/// A model instance to control instance registry
/// </summary>
/// <typeparam name="TModel">The base class for the models</typeparam>
/// <typeparam name="TControl">The base class for the controls</typeparam>
public class ModelControlRegistry<TModel, TControl> where TControl : Control where TModel : class {
    private readonly Dictionary<Type, Delegate> constructors;

    public ModelControlRegistry() {
        this.constructors = new Dictionary<Type, Delegate>();
    }

    public void RegisterType<TSpecificModel>(Func<TSpecificModel, TControl> constructor) where TSpecificModel : TModel {
        // Need to create a Func<TModel, TControl>. cannot use the parameter since generic type is too high so it's
        // incompatible and therefore impossible to use in the NewInstance method, at least without using reflection it is
        this.constructors[typeof(TSpecificModel)] = new Func<TModel, TControl>(x => constructor((TSpecificModel) x));
    }

    public void RegisterType<TSpecificModel>(Func<TControl> constructor) where TSpecificModel : TModel {
        this.constructors[typeof(TSpecificModel)] = constructor;
    }

    public bool TryGetNewInstance(TModel model, [NotNullWhen(true)] out TControl? control) {
        return (control = this.NewInstanceInternal(model, false)) != null;
    }

    public TControl NewInstance(TModel model) {
        TControl? control = this.NewInstanceInternal(model, true);
        return control ?? throw new Exception("No such content control for resource type: " + model.GetType().Name);
    }
    
    private TControl? NewInstanceInternal(TModel model, bool logBaseTypeScan) {
        if (model == null) {
            throw new ArgumentNullException(nameof(model));
        }

        bool hasLogged = false;
        // Just try to find a base control type. It should be found first try unless I forgot to register a new control type
        for (Type? type = model.GetType(); type != null; type = type.BaseType) {
            if (this.constructors.TryGetValue(type, out Delegate? function)) {
                return function is Func<TModel, TControl> biFunc ? biFunc(model) : ((Func<TControl>) function)();
            }

            if (logBaseTypeScan && !hasLogged) {
                hasLogged = true;
                Debugger.Break();
                Debug.WriteLine("Could not find control for resource type on first try. Scanning base types");
            }
        }

        return null;
    }
}