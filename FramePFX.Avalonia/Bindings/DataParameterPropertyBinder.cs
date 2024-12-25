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
using FramePFX.DataTransfer;

namespace FramePFX.Avalonia.Bindings;

/// <summary>
/// The most basic fully automatic data parameter binder. This uses the object getter/setter of both
/// the control and the parameter's accessor. It also provides converter functions for easy conversion
/// between different types (e.g. string to double and vice versa)
/// </summary>
/// <typeparam name="TModel">The type of model which implements <see cref="ITransferableData"/> and can store property values</typeparam>
public class DataParameterPropertyBinder<TModel> : BaseAutoUpdatePropertyBinder<TModel> where TModel : class, ITransferableData {
    public DataParameter? Parameter { get; }

    private readonly Func<object?, object?>? ParamToProp;
    private readonly Func<object?, object?>? PropToParam;

    public bool CanUpdateModel { get; init; } = true;

    /// <summary>
    /// Creates a new data parameter property binder
    /// </summary>
    /// <param name="property">The avalonia property, which is used to listen to property changes</param>
    /// <param name="parameter">The data parameter, used to listen to model value changes</param>
    /// <param name="parameterToProperty">Converts the parameter value to an appropriate property value (e.g. double to string)</param>
    /// <param name="propertyToParameter">Converts the property value back to the parameter value (e.g. string to double, or returns validation error)</param>
    public DataParameterPropertyBinder(AvaloniaProperty? property, DataParameter? parameter, Func<object?, object?>? parameterToProperty = null, Func<object?, object?>? propertyToParameter = null) : base(property) {
        this.Parameter = parameter;
        this.ParamToProp = parameterToProperty;
        this.PropToParam = propertyToParameter;
    }

    protected override void UpdateModelOverride() {
        if (this.CanUpdateModel && this.IsFullyAttached && this.Property != null && this.Parameter != null) {
            object? newValue = this.myControl!.GetValue(this.Property);
            this.Parameter.SetObjectValue(this.Model, this.PropToParam != null ? this.PropToParam(newValue) : newValue);
        }
    }

    protected override void UpdateControlOverride() {
        if (this.IsFullyAttached && this.Property != null && this.Parameter != null) {
            object? newValue = this.Parameter.GetObjectValue(this.Model);
            this.myControl!.SetValue(this.Property, this.ParamToProp != null ? this.ParamToProp(newValue) : newValue);
        }
    }

    private void OnDataParameterValueChanged(DataParameter parameter, ITransferableData owner) => this.UpdateControl();

    protected override void OnAttached() {
        base.OnAttached();
        this.Parameter?.AddValueChangedHandler(this.Model, this.OnDataParameterValueChanged);
    }

    protected override void OnDetached() {
        base.OnDetached();
        this.Parameter?.RemoveValueChangedHandler(this.Model, this.OnDataParameterValueChanged);
    }
}