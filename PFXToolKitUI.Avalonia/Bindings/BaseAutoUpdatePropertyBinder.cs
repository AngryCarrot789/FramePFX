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
using Avalonia.Controls;

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// A base class which implements a property changed listener for the attached control,
/// which invokes the <see cref="IBinder.UpdateModel"/> method
/// </summary>
/// <typeparam name="TModel">The model type</typeparam>
public abstract class BaseAutoUpdatePropertyBinder<TModel> : BaseBinder<TModel> where TModel : class {
    /// <summary>
    /// A property which, when its value changes on the attached control, will invoke <see cref="BaseBinder{TModel}.UpdateModel"/>.
    /// <para>
    /// This may be null if the control value change is processed elsewhere instead of automatically
    /// </para>
    /// </summary>
    public AvaloniaProperty? Property { get; }

    protected BaseAutoUpdatePropertyBinder(AvaloniaProperty? property) {
        this.Property = property;
    }

    protected override void CheckAttachControl(Control control) {
        base.CheckAttachControl(control);
        if (this.Property != null && !AvaloniaPropertyRegistry.Instance.IsRegistered(control.GetType(), this.Property)) {
            throw new InvalidOperationException($"The control cannot be attached because the property owner type is incompatible with the control type. Control '{control.GetType().Name}' is not assignable to prop owner '{this.Property.OwnerType.Name}'");
        }
    }

    // We must listen to the control's property changed event rather than add a
    // change handler for the property itself, because otherwise the handler gets
    // called for any instance. It might be cheaper to listen to the event

    protected override void OnAttached() {
        if (this.Property != null)
            this.myControl!.PropertyChanged += this.OnControlPropertyChanged;
    }

    protected override void OnDetached() {
        if (this.Property != null)
            this.myControl!.PropertyChanged -= this.OnControlPropertyChanged;
    }

    /// <summary>
    /// Invoked by the property changed handler when our property changes on the control.
    /// By default this method invokes <see cref="IBinder.UpdateModel"/>
    /// </summary>
    protected virtual void OnControlValueChanged() => this.UpdateModel();

    private void OnControlPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        if (e.Property == this.Property) {
            this.OnControlValueChanged();
        }
    }

    // private void OnPropertyValueChanged(object sender, AvaloniaPropertyChangedEventArgs e) {
    //     // Because we are adding a property changed handler as a static handler on the property
    //     // itself, it is not in reference to the actual control, therefore, we must check ourself
    //     if (ReferenceEquals(e.Sender, this.Control)) {
    //         this.UpdateModel();
    //     }
    // }
}