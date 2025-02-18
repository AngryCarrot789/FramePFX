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

using Avalonia;

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// An object binder that contains two events for updating a control and a model. These are called by invoking
/// <see cref="BaseBinder{TModel}.UpdateControl"/> and <see cref="BaseBinder{TModel}.UpdateModel"/>
/// respectively.
/// <para>
/// When a non-null property is provided, it is used to add a property changed handler on the control,
/// so that <see cref="BaseBinder{TModel}.UpdateModel"/> does not need to be called manually, hence the "auto" part
/// </para>
/// </summary>
/// <typeparam name="TModel">The type of model</typeparam>
public class AutoUpdatePropertyBinder<TModel> : BaseAutoUpdatePropertyBinder<TModel> where TModel : class {
    public event Action<IBinder<TModel>>? DoUpdateControl;
    public event Action<IBinder<TModel>>? DoUpdateModel;

    public AutoUpdatePropertyBinder(Action<IBinder<TModel>>? updateControl, Action<IBinder<TModel>>? updateModel) : this(null, updateControl, updateModel) {
    }

    public AutoUpdatePropertyBinder(AvaloniaProperty? property, Action<IBinder<TModel>>? updateControl, Action<IBinder<TModel>>? updateModel) : base(property) {
        this.DoUpdateControl = updateControl;
        this.DoUpdateModel = updateModel;
    }

    protected override void UpdateModelOverride() => this.DoUpdateModel?.Invoke(this);

    protected override void UpdateControlOverride() => this.DoUpdateControl?.Invoke(this);
}