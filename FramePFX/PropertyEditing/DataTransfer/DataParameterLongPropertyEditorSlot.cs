using System;
using FramePFX.Editors.DataTransfer;
using FramePFX.PropertyEditing.Automation;

namespace FramePFX.PropertyEditing.DataTransfer {
    public class DataParameterLongPropertyEditorSlot : DataParameterPropertyEditorSlot {
        private long value;

        public long Value {
            get => this.value;
            set {
                long oldVal = this.value;
                this.value = value;
                bool useAddition = this.IsMultiHandler;
                long change = value - oldVal;
                DataParameterLong parameter = this.DataParameter;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    ITransferableData obj = (ITransferableData) this.Handlers[i];
                    long newValue = parameter.Clamp(useAddition ? (parameter.GetValue(obj) + change) : value);
                    parameter.SetValue(obj, newValue);
                }

                this.OnValueChanged();
            }
        }

        public new DataParameterLong DataParameter => (DataParameterLong) base.DataParameter;

        public DragStepProfile StepProfile { get; }

        public DataParameterLongPropertyEditorSlot(DataParameterLong parameter, Type applicableType, string displayName, DragStepProfile stepProfile) : base(parameter, applicableType, displayName) {
            this.StepProfile = stepProfile;
        }

        public DataParameterLongPropertyEditorSlot(DataParameterLong parameter, DataParameterGeneric<bool> isEditableParameter, bool invertIsEditable, Type applicableType, string displayName, DragStepProfile stepProfile) : base(parameter, applicableType, displayName) {
            this.StepProfile = stepProfile;
            this.IsEditableParameter = isEditableParameter;
            this.InvertIsEditableForParameter = invertIsEditable;
        }

        public override void QueryValueFromHandlers() {
            this.value = GetEqualValue(this.Handlers, (x) => this.DataParameter.GetValue((ITransferableData) x), out long d) ? d : default;
        }
    }
}