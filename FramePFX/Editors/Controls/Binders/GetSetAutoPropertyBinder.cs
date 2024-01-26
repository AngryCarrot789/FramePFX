using System;
using System.Reflection;
using System.Windows;

namespace FramePFX.Editors.Controls.Binders {
    /// <summary>
    /// A binder that just requires a getter and setter for the model value and either <see cref="OnPropertyChanged"/>
    /// or <see cref="BaseObjectBinder{TModel}.OnControlValueChanged"/> to be called when the control value changes.
    /// A model value changed event handler will be auto-registered
    /// </summary>
    /// <typeparam name="TModel">The model type</typeparam>
    public class GetSetAutoPropertyBinder<TModel> : BaseObjectBinder<TModel> where TModel : class {
        private readonly EventInfo eventInfo;
        private readonly Func<IBinder<TModel>, object> getter;
        private readonly Action<IBinder<TModel>, object> setter;
        private readonly Delegate handlerInternal;

        /// <summary>
        /// The dependency property that is used when <see cref="OnPropertyChanged"/> is
        /// called, to update the model value if possible
        /// </summary>
        public DependencyProperty Property { get; set; }

        public GetSetAutoPropertyBinder(string eventName, Func<IBinder<TModel>, object> getModelValue, Action<IBinder<TModel>, object> setModelValue) {
            this.eventInfo = typeof(TModel).GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);
            if (this.eventInfo == null)
                throw new Exception("Could not find event by name: " + typeof(TModel).Name + "." + eventName);

            this.handlerInternal = BinderUtils.CreateDelegateToInvokeActionFromEvent(this.eventInfo.EventHandlerType, this.OnEvent);
            this.getter = getModelValue;
            this.setter = setModelValue;
        }

        public GetSetAutoPropertyBinder(DependencyProperty property, string eventName, Func<IBinder<TModel>, object> getModelValue, Action<IBinder<TModel>, object> setModelValue) : this(eventName, getModelValue, setModelValue) {
            this.Property = property;
        }

        public void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            if (!this.IsUpdatingControl && e.Property == this.Property && this.Property != null) {
                this.OnControlValueChanged();
            }
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
        }

        protected override void OnDetatched() {
            base.OnDetatched();
            this.eventInfo.RemoveEventHandler(this.Model, this.handlerInternal);
        }

        public bool BeginExternalControlUpdate() {
            if (this.IsAttached && this.Property != null && this.getter != null) {
                this.IsUpdatingControl = true;
                return true;
            }

            return false;
        }

        public void EndExternalControlUpdate() {
            this.IsUpdatingControl = false;
        }
    }
}