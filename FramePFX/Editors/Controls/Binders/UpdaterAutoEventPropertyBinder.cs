using System;
using System.Reflection;
using System.Windows;

namespace FramePFX.Editors.Controls.Binders {
    /// <summary>
    /// A binder that automatically handles a dependency property value change signal to update the model. A model value
    /// changed event handler will be auto-registered to tell the control value to update.
    /// The model and control value update are 2 actions passed in the constructor
    /// </summary>
    /// <typeparam name="TModel">The type of model</typeparam>
    public class UpdaterAutoEventPropertyBinder<TModel> : UpdaterPropertyBinder<TModel> where TModel : class {
        private readonly EventInfo eventInfo;
        private readonly Delegate handlerInternal;

        public UpdaterAutoEventPropertyBinder(string eventName, Action<IBinder<TModel>> updateControl, Action<IBinder<TModel>> updateModel) : this(null, eventName, updateControl, updateModel) {

        }

        public UpdaterAutoEventPropertyBinder(DependencyProperty property, string eventName, Action<IBinder<TModel>> updateControl, Action<IBinder<TModel>> updateModel) : base(property, updateControl, updateModel) {
            this.eventInfo = typeof(TModel).GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);
            if (this.eventInfo == null)
                throw new Exception("Could not find event by name: " + typeof(TModel).Name + "." + eventName);
            this.handlerInternal = InternalBinderUtils.CreateDelegateToInvokeActionFromEvent(this.eventInfo.EventHandlerType, this.OnModelValueChanged);
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