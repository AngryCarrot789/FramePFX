using System;
using FramePFX.Editors.DataTransfer;

namespace FramePFX.PropertyEditing.DataTransfer {
    public class DataParameterStringPropertyEditorSlot : DataParameterPropertyEditorSlot {
        private string value;

        public string Value {
            get => this.value;
            set {
                this.value = value;
                DataParameterString parameter = this.DataParameter;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    parameter.SetValue((ITransferableData) this.Handlers[i], value);
                }

                this.OnValueChanged();
            }
        }

        public new DataParameterString DataParameter => (DataParameterString)base.DataParameter;

        public DataParameterStringPropertyEditorSlot(DataParameterString parameter, Type applicableType, string displayName) : base(parameter, applicableType, displayName) {

        }

        public override void QueryValueFromHandlers() {
            this.value = GetEqualValue(this.Handlers, (x) => this.DataParameter.GetValue((ITransferableData) x), out string d) ? d : default;
        }
    }
}