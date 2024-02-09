using System;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Controls.Automation;
using FramePFX.Utils;

namespace FramePFX.PropertyEditing.Automation {
    public class ParameterBooleanPropertyEditorSlot : ParameterPropertyEditorSlot {
        private bool? value;

        public bool? Value {
            get => this.value;
            set {
                if (value == this.value)
                    return;
                this.value = value;
                object boxedValue = value.HasValue ? value.Value.Box() : BoolBox.False;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    AutomatedUtils.SetDefaultKeyFrameOrAddNew((IAutomatable) this.Handlers[i], base.Parameter, boxedValue);
                }

                this.OnValueChanged();
            }
        }

        public new ParameterBoolean Parameter => (ParameterBoolean) base.Parameter;

        public ParameterBooleanPropertyEditorSlot(ParameterBoolean parameter, Type applicableType, string displayName) : base(parameter, applicableType, displayName) {
        }

        protected override void QueryValueFromHandlers() {
            this.value = GetEqualValue(this.Handlers, (x) => this.Parameter.GetCurrentValue((IAutomatable) x), out bool d) ? d : (bool?) null;
        }
    }
}