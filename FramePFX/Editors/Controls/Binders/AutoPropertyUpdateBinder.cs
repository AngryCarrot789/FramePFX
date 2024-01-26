using System;
using System.Reflection;
using System.Windows;

namespace FramePFX.Editors.Controls.Binders {
    /// <summary>
    /// A binder that relies on either <see cref="OnPropertyChanged"/> or <see cref="BaseObjectBinder{TModel}.OnControlValueChanged"/>
    /// to be called when the control value changes. A model value changed event handler will be auto-registered to tell the control
    /// value to update. The model and control value update are 2 actions passed in the constructor
    /// </summary>
    /// <typeparam name="TModel">The type of model</typeparam>
    public class AutoPropertyUpdateBinder<TModel> : PropertyUpdateBinder<TModel> where TModel : class {
        private readonly EventInfo eventInfo;
        private readonly Delegate handlerInternal;

        public AutoPropertyUpdateBinder(string eventName, Action<IBinder<TModel>> updateControl, Action<IBinder<TModel>> updateModel) : base(updateControl, updateModel) {
            this.eventInfo = typeof(TModel).GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);
            if (this.eventInfo == null)
                throw new Exception("Could not find event by name: " + typeof(TModel).Name + "." + eventName);
            this.handlerInternal = BinderUtils.CreateDelegateToInvokeActionFromEvent(this.eventInfo.EventHandlerType, this.OnModelValueChanged);
        }

        public AutoPropertyUpdateBinder(DependencyProperty property, string eventName, Action<IBinder<TModel>> updateControl, Action<IBinder<TModel>> updateModel) : this(eventName, updateControl, updateModel) {
            this.Property = property;
        }

        protected override void OnAttached() {
            base.OnAttached();
            this.eventInfo.AddEventHandler(this.Model, this.handlerInternal);
        }

        protected override void OnDetatched() {
            base.OnDetatched();
            this.eventInfo.RemoveEventHandler(this.Model, this.handlerInternal);
        }
    }
}