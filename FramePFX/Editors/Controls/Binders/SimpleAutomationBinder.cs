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

        [SwitchAutomationDataType]
        protected override void UpdateControlCore() {
            object value;
            switch (this.Parameter.DataType) {
                case AutomationDataType.Float: value = ((ParameterFloat) this.Parameter).GetEffectiveValue(this.Model); break;
                case AutomationDataType.Double: value = ((ParameterDouble) this.Parameter).GetEffectiveValue(this.Model); break;
                case AutomationDataType.Long: value = ((ParameterLong) this.Parameter).GetEffectiveValue(this.Model); break;
                case AutomationDataType.Boolean: value = ((ParameterBoolean) this.Parameter).GetEffectiveValue(this.Model); break;
                default: throw new ArgumentOutOfRangeException();
            }

            this.Control.SetValue(this.Property, value);
        }
    }
}