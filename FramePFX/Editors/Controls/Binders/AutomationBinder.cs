using System;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;

namespace FramePFX.Editors.Controls.Binders {
    public sealed class AutomationBinder<TModel> : BaseObjectBinder<TModel> where TModel : class, IHaveTimeline, IAutomatable {
        private readonly ParameterChangedEventHandler handler;

        public Parameter Parameter { get; }

        public event Action<AutomationBinder<TModel>> UpdateModel;
        public event Action<AutomationBinder<TModel>> UpdateControl;

        public AutomationBinder(Parameter parameter) {
            this.handler = this.OnParameterValueChanged;
            this.Parameter = parameter;
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
            this.UpdateModel?.Invoke(this);
        }

        protected override void UpdateControlCore() {
            this.UpdateControl?.Invoke(this);
        }
    }
}