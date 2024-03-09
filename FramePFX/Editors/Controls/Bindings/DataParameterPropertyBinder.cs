//
// Copyright (c) 2023-2024 REghZy
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
using System.ComponentModel;
using System.Windows;
using FramePFX.Editors.DataTransfer;

namespace FramePFX.Editors.Controls.Bindings
{
    /// <summary>
    /// A binder that binds between a data parameter and a dependency property, completely automated requiring no external signaling
    /// </summary>
    /// <typeparam name="TModel">The model type</typeparam>
    public class DataParameterPropertyBinder<TModel> : BaseBinder<TModel> where TModel : class, ITransferableData
    {
        /// <summary>
        /// Gets the property that is used to listen to property value changed notifications on the attached
        /// control. May be null if <see cref="BaseBinder{TModel}.OnControlValueChanged"/> is called manually
        /// </summary>
        public DependencyProperty Property { get; }

        public DataParameter Parameter { get; }

        private DependencyPropertyDescriptor descriptor;

        public DataParameterPropertyBinder(DependencyProperty property, DataParameter parameter)
        {
            this.Property = property ?? throw new ArgumentNullException(nameof(property));
            this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        }

        // we can get away with using the object API for parameters since the DP system boxes everything anyway

        protected override void UpdateModelCore()
        {
            if (this.IsAttached)
            {
                object newValue = this.Control.GetValue(this.Property);
                this.Parameter.SetObjectValue(this.Model, newValue);
            }
        }

        protected override void UpdateControlCore()
        {
            if (this.IsAttached)
            {
                object newValue = this.Parameter.GetObjectValue(this.Model);
                this.Control.SetValue(this.Property, newValue);
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.Parameter.AddValueChangedHandler(this.Model, this.OnDataParameterValueChanged);
            this.descriptor = DependencyPropertyDescriptor.FromProperty(this.Property, this.Control.GetType());
            this.descriptor.AddValueChanged(this.Control, this.OnPropertyValueChanged);
        }

        protected override void OnDetached()
        {
            base.OnDetached();
            this.Parameter.RemoveValueChangedHandler(this.Model, this.OnDataParameterValueChanged);
            this.descriptor.RemoveValueChanged(this.Control, this.OnPropertyValueChanged);
            this.descriptor = null;
        }

        private void OnDataParameterValueChanged(DataParameter parameter, ITransferableData owner) => this.OnModelValueChanged();

        private void OnPropertyValueChanged(object sender, EventArgs e) => this.OnControlValueChanged();
    }
}