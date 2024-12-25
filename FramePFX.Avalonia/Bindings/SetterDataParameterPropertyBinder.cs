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

public class SetterDataParameterPropertyBinder<TModel, TValue> : BaseAutoUpdatePropertyBinder<TModel> where TModel : class, ITransferableData {
    public DataParameter<TValue> Parameter { get; }

    public delegate bool SetterFunction(SetterDataParameterPropertyBinder<TModel, TValue> binder, object? value);

    private readonly Func<TValue?, object?> ParamToPropForGetter;
    private readonly SetterFunction Setter;
    private bool hasError;

    /// <summary>
    /// Gets if the error condition is set, meaning, the control has entered a value that cannot be parsed and therefore the parameter value could not be updated
    /// </summary>
    public bool HasError {
        get => this.hasError;
        private set {
            if (this.hasError == value)
                return;

            this.hasError = value;
            this.HasErrorChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? HasErrorChanged;

    /// <summary>
    /// Creates a new data parameter property binder
    /// </summary>
    /// <param name="property">The avalonia property, which is used to listen to property changes</param>
    /// <param name="parameter">The data parameter, used to listen to model value changes</param>
    /// <param name="parameterToProperty">Converts the parameter value to an appropriate property value (e.g. double to string)</param>
    /// <param name="propertyToParameter">Converts the property value back to the parameter value (e.g. string to double, or returns validation error)</param>
    public SetterDataParameterPropertyBinder(AvaloniaProperty? property, DataParameter<TValue> parameter, Func<TValue?, object?> parameterToProperty, SetterFunction propToParam) : base(property) {
        this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        this.ParamToPropForGetter = parameterToProperty ?? throw new ArgumentNullException(nameof(parameterToProperty));
        this.Setter = propToParam ?? throw new ArgumentNullException(nameof(propToParam));
    }

    protected override void UpdateModelOverride() {
        if (this.IsFullyAttached && this.Property != null) {
            object? newValue = this.myControl!.GetValue(this.Property);
            this.HasError = this.Setter(this, newValue);
        }
    }

    protected override void UpdateControlOverride() {
        if (this.IsFullyAttached && this.Property != null) {
            TValue? newValue = this.Parameter.GetValue(this.Model);
            this.myControl!.SetValue(this.Property, this.ParamToPropForGetter(newValue));
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