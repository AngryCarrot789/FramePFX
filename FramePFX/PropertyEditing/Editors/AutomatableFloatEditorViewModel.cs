using System;
using System.Collections.Generic;
using FramePFX.Automation.Keys;
using FramePFX.Automation.ViewModels;

namespace FramePFX.PropertyEditing.Editors {
    public class AutomatableFloatEditorViewModel : AutomatablePropertyEditorViewModel<float> {
        public float Min { get; }

        public float Max { get; }

        public AutomatableFloatEditorViewModel(AutomationKey automationKey, Func<IAutomatableViewModel, float> getter, Action<IAutomatableViewModel, float> setter) :
            base(automationKey, getter, setter) {
            KeyDescriptorFloat desc = (KeyDescriptorFloat) automationKey.Descriptor;
            this.Min = float.IsInfinity(desc.Minimum) ? float.MinValue : desc.Minimum;
            this.Max = float.IsInfinity(desc.Maximum) ? float.MaxValue : desc.Maximum;
        }

        protected override void OnValueChanged(IReadOnlyList<IAutomatableViewModel> handlers, float oldValue, float value) {
            int count = handlers.Count;
            if (count == 1) {
                this.SetValuesAndHistory(value);
            }
            else {
                float change = value - oldValue;
                for (int i = 0; i < count; i++) {
                    this.SetValueAndHistory(i, this.Getter(handlers[i]) + change);
                }
            }
        }

        protected override void OnResetValue(IReadOnlyList<IAutomatableViewModel> handlers) {
            this.SetValuesAndHistory(((KeyDescriptorFloat) this.AutomationKey.Descriptor).DefaultValue);
        }
    }
}