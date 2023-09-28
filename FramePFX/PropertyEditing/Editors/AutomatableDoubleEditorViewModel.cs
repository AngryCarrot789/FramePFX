using System;
using System.Collections.Generic;
using FramePFX.Automation.Keys;
using FramePFX.Automation.ViewModels;

namespace FramePFX.PropertyEditing.Editors {
    public class AutomatableDoubleEditorViewModel : AutomatablePropertyEditorViewModel<double> {
        public double Min { get; }

        public double Max { get; }

        public AutomatableDoubleEditorViewModel(AutomationKey automationKey, Func<IAutomatableViewModel, double> getter, Action<IAutomatableViewModel, double> setter) :
            base(automationKey, getter, setter) {
            KeyDescriptorDouble desc = (KeyDescriptorDouble) automationKey.Descriptor;
            this.Min = double.IsInfinity(desc.Minimum) ? double.MinValue : desc.Minimum;
            this.Max = double.IsInfinity(desc.Maximum) ? double.MaxValue : desc.Maximum;
        }

        protected override void OnValueChanged(IReadOnlyList<IAutomatableViewModel> handlers, double oldValue, double value) {
            int count = handlers.Count;
            if (count == 1) {
                this.SetValuesAndHistory(value);
            }
            else {
                double change = value - oldValue;
                for (int i = 0; i < count; i++) {
                    this.SetValueAndHistory(i, this.Getter(handlers[i]) + change);
                }
            }
        }

        protected override void OnResetValue(IReadOnlyList<IAutomatableViewModel> handlers) {
            this.SetValuesAndHistory(((KeyDescriptorDouble) this.AutomationKey.Descriptor).DefaultValue);
        }
    }
}