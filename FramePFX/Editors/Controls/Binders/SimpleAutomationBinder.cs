using System;
using System.Windows;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;

namespace FramePFX.Editors.Controls.Binders {
    public class SimpleAutomationBinder<TModel> : BaseObjectBinder<TModel> where TModel : class, IHaveTimeline, IAutomatable {
        private readonly ParameterChangedEventHandler handler;

        public Parameter Parameter { get; }

        public DependencyProperty Property { get; }

        public event Action UpdateModel;

        public SimpleAutomationBinder(Parameter parameter, DependencyProperty property) {
            this.handler = this.OnParameterValueChanged;
            this.Parameter = parameter;
            this.Property = property;
        }

        private void OnParameterValueChanged(AutomationSequence sequence) => this.OnModelValueChanged();

        protected override void OnAttached() {
            base.OnAttached();
            this.Model.AutomationData.AddParameterChangedHandler(this.Parameter, this.handler);
        }

        protected override void OnDetatched() {
            base.OnDetatched();
            this.Model.AutomationData.RemoveParameterChangedHandler(this.Parameter, this.handler);
        }

        protected override void UpdateModelCore() {
            this.UpdateModel?.Invoke();
        }

        protected override void UpdateControlCore() {
            object value = this.Parameter.GetCurrentObjectValue(this.Model);
            this.Control.SetValue(this.Property, value);
        }
    }
}