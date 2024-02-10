using System;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Controls.Automation;

namespace FramePFX.PropertyEditing.Automation {
    public class ParameterLongPropertyEditorSlot : ParameterPropertyEditorSlot {
        private long value;

        public long Value {
            get => this.value;
            set {
                long oldVal = this.value;
                this.value = value;
                bool useAddition = this.IsMultiHandler;
                long change = value - oldVal;
                ParameterLong parameter = this.Parameter;
                ParameterDescriptorLong pdesc = parameter.Descriptor;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    IAutomatable obj = (IAutomatable) this.Handlers[i];
                    long newValue = pdesc.Clamp(useAddition ? (parameter.GetCurrentValue(obj) + change) : value);
                    AutomatedUtils.SetDefaultKeyFrameOrAddNew(obj, parameter, newValue);
                }

                this.OnValueChanged();
            }
        }

        public new ParameterLong Parameter => (ParameterLong)base.Parameter;

        public DragStepProfile StepProfile { get; }

        public ParameterLongPropertyEditorSlot(ParameterLong parameter, Type applicableType, string displayName, DragStepProfile stepProfile) : base(parameter, applicableType, displayName) {
            this.StepProfile = stepProfile;
        }

        protected override void QueryValueFromHandlers() {
            this.value = GetEqualValue(this.Handlers, (x) => this.Parameter.GetCurrentValue((IAutomatable) x), out long d) ? d : default;
        }
    }
}