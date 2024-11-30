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

namespace FramePFX.Avalonia.Bindings;

/// <summary>
/// A binder that inherits <see cref="BaseAutoUpdateAndEventPropertyBinder{TModel}"/> that
/// uses two action events for updating the control and the model
/// </summary>
/// <typeparam name="TModel">The type of model</typeparam>
public class AutoUpdateAndEventPropertyBinder<TModel> : BaseAutoUpdateAndEventPropertyBinder<TModel> where TModel : class {
    public event Action<IBinder<TModel>>? DoUpdateControl;
    public event Action<IBinder<TModel>>? DoUpdateModel;

    public AutoUpdateAndEventPropertyBinder(string eventName, Action<IBinder<TModel>>? updateControl, Action<IBinder<TModel>>? updateModel) : this(null, eventName, updateControl, updateModel) {
    }

    public AutoUpdateAndEventPropertyBinder(AvaloniaProperty? property, string eventName, Action<IBinder<TModel>>? updateControl, Action<IBinder<TModel>>? updateModel) : base(property, eventName) {
        this.DoUpdateControl = updateControl;
        this.DoUpdateModel = updateModel;
    }

    protected override void UpdateModelOverride() => this.DoUpdateModel?.Invoke(this);

    protected override void UpdateControlOverride() => this.DoUpdateControl?.Invoke(this);
}