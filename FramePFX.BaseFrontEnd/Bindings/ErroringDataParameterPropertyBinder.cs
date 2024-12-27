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
using FramePFX.DataTransfer;

namespace FramePFX.BaseFrontEnd.Bindings;

public class ErroringDataParameterPropertyBinder<TModel, TValue> : BaseAutoUpdatePropertyBinder<TModel> where TModel : class, ITransferableData {
    private bool hasError;

    public delegate bool PropertyToParameterFunction(object? prop, out TValue? value);

    public DataParameter<TValue> Parameter { get; }

    private readonly Func<TValue?, object?> ParamToProp;
    private readonly PropertyToParameterFunction PropToParam;

    public bool HasError {
        get => this.hasError;
        set {
            if (this.hasError == value)
                return;

            this.hasError = value;
            this.IsErrorChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? IsErrorChanged;

    /// <summary>
    /// Creates a new data parameter property binder
    /// </summary>
    /// <param name="property">The avalonia property, which is used to listen to property changes</param>
    /// <param name="parameter">The data parameter, used to listen to model value changes</param>
    /// <param name="parameterToProperty">Converts the parameter value to an appropriate property value (e.g. double to string)</param>
    /// <param name="propertyToParameter">Converts the property value back to the parameter value (e.g. string to double, or returns validation error)</param>
    public ErroringDataParameterPropertyBinder(AvaloniaProperty? property, DataParameter<TValue> parameter, Func<TValue?, object?> parameterToProperty, PropertyToParameterFunction propertyToParameter) : base(property) {
        this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        this.ParamToProp = parameterToProperty ?? throw new ArgumentNullException(nameof(parameterToProperty));
        this.PropToParam = propertyToParameter ?? throw new ArgumentNullException(nameof(propertyToParameter));
    }

    protected override void UpdateModelOverride() {
        if (this.IsFullyAttached && this.Property != null) {
            object? newValue = this.myControl!.GetValue(this.Property);
            if (this.PropToParam(newValue, out TValue? theValue)) {
                this.Parameter.SetObjectValue(this.Model, theValue);
                this.HasError = false;
            }
            else {
                this.HasError = true;
            }
        }
    }

    protected override void UpdateControlOverride() {
        if (this.IsFullyAttached && this.Property != null) {
            TValue? newValue = this.Parameter.GetValue(this.Model);
            this.myControl!.SetValue(this.Property, this.ParamToProp(newValue));
        }
    }

    private void OnDataParameterValueChanged(DataParameter parameter, ITransferableData owner) => this.UpdateControl();

    protected override void OnAttached() {
        base.OnAttached();
        this.Parameter.AddValueChangedHandler(this.Model, this.OnDataParameterValueChanged);
    }

    protected override void OnDetached() {
        base.OnDetached();
        this.Parameter.RemoveValueChangedHandler(this.Model, this.OnDataParameterValueChanged);
    }
}