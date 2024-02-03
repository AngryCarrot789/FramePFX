using System;
using FramePFX.Editors.DataTransfer;
using FramePFX.PropertyEditing.Automation;

namespace FramePFX.PropertyEditing.DataTransfer {
    public class DataParameterFloatPropertyEditorSlot : DataParameterPropertyEditorSlot {
        private float value;

        public float Value {
            get => this.value;
            set {
                float oldVal = this.value;
                this.value = value;
                bool useAddition = this.IsMultiHandler;
                float change = value - oldVal;
                DataParameterFloat parameter = this.DataParameter;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    ITransferableData obj = (ITransferableData) this.Handlers[i];
                    float newValue = parameter.Clamp(useAddition ? (parameter.GetValue(obj) + change) : value);
                    parameter.SetValue(obj, newValue);
                }

                this.OnValueChanged();
            }
        }

        public new DataParameterFloat DataParameter => (DataParameterFloat)base.DataParameter;

        public DragStepProfile StepProfile { get; }

        public DataParameterFloatPropertyEditorSlot(DataParameterFloat parameter, Type applicableType, string displayName, DragStepProfile stepProfile) : base(parameter, applicableType, displayName) {
            this.StepProfile = stepProfile;
        }

        public override void QueryValueFromHandlers() {
            this.value = GetEqualValue(this.Handlers, (x) => this.DataParameter.GetValue((ITransferableData) x), out float d) ? d : default;
        }
    }
}