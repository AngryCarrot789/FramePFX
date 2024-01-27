using System;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Controls.Automation;

namespace FramePFX.PropertyEditing.Standard {
    public class ParameterFloatPropertyEditorSlot : ParameterPropertyEditorSlot {
        private float value;

        public float Value {
            get => this.value;
            set {
                float oldVal = this.value;
                this.value = value;
                bool useAddition = this.IsMultiHandler;
                float change = value - oldVal;
                ParameterFloat parameter = this.Parameter;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    IAutomatable obj = (IAutomatable) this.Handlers[i];
                    float newClipValue = parameter.Descriptor.Clamp(useAddition ? (parameter.GetEffectiveValue(obj) + change) : value);
                    AutomatedUtils.SetDefaultKeyFrameOrAddNew(obj, parameter, newClipValue);
                }

                this.OnValueChanged();
            }
        }

        public new ParameterFloat Parameter => (ParameterFloat)base.Parameter;

        public DragStepProfile StepProfile { get; }

        public ParameterFloatPropertyEditorSlot(ParameterFloat parameter, Type applicableType, string displayName, DragStepProfile stepProfile) : base(parameter, applicableType, displayName) {
            this.StepProfile = stepProfile;
        }

        public override void QueryValueFromHandlers() {
            this.value = GetEqualValue(this.Handlers, (x) => this.Parameter.GetEffectiveValue((IAutomatable) x), out float d) ? d : default;
        }
    }
}