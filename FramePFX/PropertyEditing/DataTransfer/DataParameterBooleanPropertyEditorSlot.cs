using System;
using FramePFX.Editors.DataTransfer;

namespace FramePFX.PropertyEditing.DataTransfer {
    public class DataParameterBooleanPropertyEditorSlot : DataParameterPropertyEditorSlot {
        private bool value;

        public bool Value {
            get => this.value;
            set {
                this.value = value;
                DataParameterBoolean parameter = this.DataParameter;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    parameter.SetValue((ITransferableData) this.Handlers[i], value);
                }

                this.OnValueChanged();
            }
        }

        public new DataParameterBoolean DataParameter => (DataParameterBoolean)base.DataParameter;

        public DataParameterBooleanPropertyEditorSlot(DataParameterBoolean parameter, Type applicableType, string displayName) : base(parameter, applicableType, displayName) {
            
        }

        public override void QueryValueFromHandlers() {
            this.value = GetEqualValue(this.Handlers, (x) => this.DataParameter.GetValue((ITransferableData) x), out bool d) ? d : default;
        }
    }
}