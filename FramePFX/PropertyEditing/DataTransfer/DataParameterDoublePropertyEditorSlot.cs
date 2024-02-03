using System;
using FramePFX.Editors.DataTransfer;
using FramePFX.PropertyEditing.Automation;

namespace FramePFX.PropertyEditing.DataTransfer {
    public class DataParameterDoublePropertyEditorSlot : DataParameterPropertyEditorSlot {
        private double value;

        public double Value {
            get => this.value;
            set {
                double oldVal = this.value;
                this.value = value;
                bool useAddition = this.IsMultiHandler;
                double change = value - oldVal;
                DataParameterDouble parameter = this.DataParameter;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    ITransferableData obj = (ITransferableData) this.Handlers[i];
                    double newValue = parameter.Clamp(useAddition ? (parameter.GetValue(obj) + change) : value);
                    parameter.SetValue(obj, newValue);
                }

                this.OnValueChanged();
            }
        }

        public new DataParameterDouble DataParameter => (DataParameterDouble) base.DataParameter;

        public DragStepProfile StepProfile { get; }

        public DataParameterDoublePropertyEditorSlot(DataParameterDouble parameter, Type applicableType, string displayName, DragStepProfile stepProfile) : base(parameter, applicableType, displayName) {
            this.StepProfile = stepProfile;
        }

        public DataParameterDoublePropertyEditorSlot(DataParameterDouble parameter, DataParameterGeneric<bool> isEditableParameter, bool invertIsEditable, Type applicableType, string displayName, DragStepProfile stepProfile) : base(parameter, applicableType, displayName) {
            this.StepProfile = stepProfile;
            this.IsEditableParameter = isEditableParameter;
            this.InvertIsEditableForParameter = invertIsEditable;
        }

        public override void QueryValueFromHandlers() {
            this.value = GetEqualValue(this.Handlers, (x) => this.DataParameter.GetValue((ITransferableData) x), out double d) ? d : default;
        }
    }
}