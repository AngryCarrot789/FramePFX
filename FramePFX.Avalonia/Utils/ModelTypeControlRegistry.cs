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
using Avalonia.Controls;

namespace FramePFX.Avalonia.Utils;

/// <summary>
/// A model type to control instance registry
/// </summary>
/// <typeparam name="TControl">The base class for the controls</typeparam>
public class ModelTypeControlRegistry<TControl> where TControl : Control
{
    private readonly Dictionary<Type, Func<TControl>> constructors;

    public ModelTypeControlRegistry()
    {
        this.constructors = new Dictionary<Type, Func<TControl>>();
    }

    public void RegisterType(Type modelType, Func<TControl> constructor)
    {
        this.constructors[modelType] = constructor;
    }

    public TControl NewInstance(Type modelType)
    {
        if (modelType == null)
        {
            throw new ArgumentNullException(nameof(modelType));
        }

        // Just try to find a base control type. It should be found first try unless I forgot to register a new control type
        bool hasLogged = false;
        for (Type? type = modelType; type != null; type = type.BaseType)
        {
            if (this.constructors.TryGetValue(type, out Func<TControl>? func))
            {
                return func();
            }

            if (!hasLogged)
            {
                hasLogged = true;
                Debugger.Break();
                Debug.WriteLine("Could not find control for resource type on first try. Scanning base types");
            }
        }

        throw new Exception("No such content control for resource type: " + modelType.Name);
    }
}