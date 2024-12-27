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
using FramePFX.Utils.Accessing;

namespace FramePFX.BaseFrontEnd.Bindings;

/// <summary>
/// A basic binder automatically listens for model value and control property changes, and uses
/// a <see cref="ValueAccessor{TValue}"/> to update the control and model values respectively
/// </summary>
/// <typeparam name="TModel">The model type</typeparam>
/// <typeparam name="TValue">The value type</typeparam>
public class AccessorAutoUpdateAndEventPropertyBinder<TModel, TValue> : BaseAutoUpdateAndEventPropertyBinder<TModel> where TModel : class {
    private readonly ValueAccessor<TValue> accessor;

    public new AvaloniaProperty<TValue>? Property => (AvaloniaProperty<TValue>?) base.Property;

    public AccessorAutoUpdateAndEventPropertyBinder(AvaloniaProperty<TValue>? property, string eventName, ValueAccessor<TValue> accessor) : base(property, eventName) {
        this.accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
    }

    private void OnEvent() => this.UpdateControl();

    protected override void UpdateModelOverride() {
        if (this.IsFullyAttached && base.Property != null) {
            object? newValue = this.myControl!.GetValue(base.Property);
            this.accessor.SetObjectValue(this.myModel!, newValue);
        }
    }

    protected override void UpdateControlOverride() {
        if (this.IsFullyAttached && base.Property != null) {
            object? newValue = this.accessor.GetObjectValue(this.myModel!);
            this.myControl!.SetValue(base.Property!, newValue);
        }
    }
}