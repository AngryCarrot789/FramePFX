using System;
using System.Numerics;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Controls.Automation;

namespace FramePFX.PropertyEditing.Automation {
    public class ParameterVector2PropertyEditorSlot : ParameterPropertyEditorSlot {
        private Vector2 value;

        public Vector2 Value {
            get => this.value;
            set {
                Vector2 oldVal = this.value;
                this.value = value;
                bool useAddition = this.IsMultiHandler;
                Vector2 change = value - oldVal;
                ParameterVector2 parameter = this.Parameter;
                ParameterDescriptorVector2 pdesc = parameter.Descriptor;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    IAutomatable obj = (IAutomatable) this.Handlers[i];
                    Vector2 newValue = pdesc.Clamp(useAddition ? (parameter.GetCurrentValue(obj) + change) : value);
                    AutomatedUtils.SetDefaultKeyFrameOrAddNew(obj, parameter, newValue);
                }

                this.OnValueChanged();
            }
        }

        public new ParameterVector2 Parameter => (ParameterVector2)base.Parameter;

        public DragStepProfile StepProfile { get; }

        public ParameterVector2PropertyEditorSlot(ParameterVector2 parameter, Type applicableType, string displayName, DragStepProfile stepProfile) : base(parameter, applicableType, displayName) {
            this.StepProfile = stepProfile;
        }

        protected override void QueryValueFromHandlers() {
            this.value = GetEqualValue(this.Handlers, (x) => this.Parameter.GetCurrentValue((IAutomatable) x), out Vector2 d) ? d : default;
        }
    }
}