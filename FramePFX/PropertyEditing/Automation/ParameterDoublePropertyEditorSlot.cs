using System;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Controls.Automation;

namespace FramePFX.PropertyEditing.Automation {
    public class ParameterDoublePropertyEditorSlot : ParameterPropertyEditorSlot {
        private double value;

        public double Value {
            get => this.value;
            set {
                double oldVal = this.value;
                this.value = value;
                bool useAddition = this.IsMultiHandler;
                double change = value - oldVal;
                ParameterDouble parameter = this.Parameter;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    IAutomatable obj = (IAutomatable) this.Handlers[i];
                    double newValue = parameter.Descriptor.Clamp(useAddition ? (parameter.GetCurrentValue(obj) + change) : value);
                    AutomatedUtils.SetDefaultKeyFrameOrAddNew(obj, parameter, newValue);
                }

                this.OnValueChanged();
            }
        }

        public new ParameterDouble Parameter => (ParameterDouble)base.Parameter;

        public DragStepProfile StepProfile { get; }

        public ParameterDoublePropertyEditorSlot(ParameterDouble parameter, Type applicableType, string displayName, DragStepProfile stepProfile) : base(parameter, applicableType, displayName) {
            this.StepProfile = stepProfile;
        }

        public override void QueryValueFromHandlers() {
            this.value = GetEqualValue(this.Handlers, (x) => this.Parameter.GetCurrentValue((IAutomatable) x), out double d) ? d : default;
        }
    }
}