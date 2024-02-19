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
using System.Reflection;
using System.Windows;
using FramePFX.Editors.Automation.Params;

namespace FramePFX.Editors.Controls.Binders {
    /// <summary>
    /// A standard basic binder that uses a getter and setter for the model value, adds and removes a dynamically generated event
    /// handler for the model value changed event, and registers a dependency property changed event handler for the control.
    /// </summary>
    /// <typeparam name="TModel">The model type</typeparam>
    public class GetSetAutoEventPropertyBinder<TModel> : BaseBinder<TModel> where TModel : class {
        private readonly EventInfo eventInfo;
        private readonly Func<IBinder<TModel>, object> getter;
        private readonly Action<IBinder<TModel>, object> setter;
        private readonly Delegate handlerInternal;
        private DependencyPropertyDescriptor descriptor;

        /// <summary>
        /// Gets the property that is used to listen to property value changed notifications on the attached
        /// control. May be null if <see cref="BaseBinder{TModel}.OnControlValueChanged"/> is called manually
        /// </summary>
        public DependencyProperty Property { get; }

        public GetSetAutoEventPropertyBinder(DependencyProperty property, string eventName, Func<IBinder<TModel>, object> getModelValue, Action<IBinder<TModel>, object> setModelValue) {
            this.eventInfo = typeof(TModel).GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);
            if (this.eventInfo == null)
                throw new Exception("Could not find event by name: " + typeof(TModel).Name + "." + eventName);

            this.handlerInternal = InternalBinderUtils.CreateDelegateToInvokeActionFromEvent(this.eventInfo.EventHandlerType, this.OnEvent);
            this.getter = getModelValue;
            this.setter = setModelValue;
            this.Property = property;
        }

        public static GetSetAutoEventPropertyBinder<TModel> ForAccessor<TValue>(DependencyProperty property, string eventName, ValueAccessor<TValue> accessor) {
            return new GetSetAutoEventPropertyBinder<TModel>(property, eventName, b => accessor.GetObjectValue(b.Model), (b, v) => accessor.SetObjectValue(b.Model, v));
        }

        private void OnEvent() => this.OnModelValueChanged();

        protected override void UpdateModelCore() {
            if (this.IsAttached && this.Property != null && this.setter != null) {
                object newValue = this.Control.GetValue(this.Property);
                this.setter(this, newValue);
            }
        }

        protected override void UpdateControlCore() {
            if (this.IsAttached && this.Property != null && this.getter != null) {
                object newValue = this.getter(this);
                this.Control.SetValue(this.Property, newValue);
            }
        }

        protected override void OnAttached() {
            base.OnAttached();
            this.eventInfo.AddEventHandler(this.Model, this.handlerInternal);
            if (this.Property != null) {
                this.descriptor = DependencyPropertyDescriptor.FromProperty(this.Property, this.Control.GetType());
                this.descriptor.AddValueChanged(this.Control, this.OnPropertyValueChanged);
            }
        }

        protected override void OnDetatched() {
            base.OnDetatched();
            this.eventInfo.RemoveEventHandler(this.Model, this.handlerInternal);
            if (this.descriptor != null) {
                this.descriptor.RemoveValueChanged(this.Control, this.OnPropertyValueChanged);
                this.descriptor = null;
            }
        }

        private void OnPropertyValueChanged(object sender, EventArgs e) {
            if (!this.IsUpdatingControl) {
                this.OnControlValueChanged();
            }
        }
    }
}