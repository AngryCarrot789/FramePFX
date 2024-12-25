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
using Avalonia;
using Avalonia.Controls;
using FramePFX.Utils.Accessing;

namespace FramePFX.Avalonia.Bindings;

public static class Binders {
    public static AccessorAutoUpdateAndEventPropertyBinder<TModel, TValue> AccessorAEDPLinq<TModel, TValue>(AvaloniaProperty<TValue> property, string eventName, string propertyOrFieldName) where TModel : class {
        // Uses cached accessor
        return AccessorAEDP<TModel, TValue>(property, eventName, ValueAccessors.LinqExpression<TValue>(typeof(TModel), propertyOrFieldName, true));
    }

    public static AccessorAutoUpdateAndEventPropertyBinder<TModel, TValue> AccessorAEDPFastStartup<TModel, TValue>(AvaloniaProperty<TValue> property, string eventName, string propertyOrFieldName) where TModel : class {
        // Uses cached accessor
        return AccessorAEDP<TModel, TValue>(property, eventName, ValueAccessors.FastStartupAccessor<TValue>(typeof(TModel), propertyOrFieldName));
    }

    public static AccessorAutoUpdateAndEventPropertyBinder<TModel, TValue> AccessorAEDP<TModel, TValue>(AvaloniaProperty<TValue> property, string eventName, ValueAccessor<TValue> accessor) where TModel : class {
        return new AccessorAutoUpdateAndEventPropertyBinder<TModel, TValue>(property, eventName, accessor);
    }

    public static AutoUpdateAndEventPropertyBinder<TModel> AutoUpdateAndEvent<TModel>(AvaloniaProperty property, string eventName, Action<IBinder<TModel>>? updateControl, Action<IBinder<TModel>>? updateModel) where TModel : class {
        return new AutoUpdateAndEventPropertyBinder<TModel>(property, eventName, updateControl, updateModel);
    }

    public static void AttachControls<TModel>(Control control, params IBinder<TModel>[] binders) where TModel : class {
        foreach (IBinder<TModel> b in binders) {
            b.AttachControl(control);
        }
    }

    public static void AttachModels<TModel>(TModel model, params IBinder<TModel>[] binders) where TModel : class {
        foreach (IBinder<TModel> b in binders) {
            b.AttachModel(model);
        }
    }

    public static void DetachModels<TModel>(params IBinder<TModel>[] binders) where TModel : class {
        foreach (IBinder<TModel> b in binders) {
            b.DetachModel();
        }
    }
}