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
/// A binder similar to <see cref="AccessorAutoUpdateAndEventPropertyBinder{TModel,TValue}"/> except
/// it uses a getter and setter method passed in the constructor, rather than a value accessor 
/// </summary>
/// <typeparam name="TModel">The model type</typeparam>
public class GetSetAutoUpdateAndEventPropertyBinder<TModel> : BaseAutoUpdateAndEventPropertyBinder<TModel> where TModel : class {
    private readonly Func<IBinder<TModel>, object?>? getter;
    private readonly Action<IBinder<TModel>, object?>? setter;

    public GetSetAutoUpdateAndEventPropertyBinder(AvaloniaProperty property, string eventName, Func<IBinder<TModel>, object?>? getModelValue, Action<IBinder<TModel>, object?>? setModelValue) : base(property, eventName) {
        this.getter = getModelValue;
        this.setter = setModelValue;
    }

    protected override void UpdateModelOverride() {
        if (this.IsFullyAttached && this.Property != null && this.setter != null) {
            object? newValue = this.myControl!.GetValue(this.Property);
            this.setter(this, newValue);
        }
    }

    protected override void UpdateControlOverride() {
        if (this.IsFullyAttached && this.Property != null && this.getter != null) {
            object? newValue = this.getter(this);
            this.myControl!.SetValue(this.Property, newValue);
        }
    }
}