using System;
using System.Collections.Generic;
using FramePFX.Automation.Keys;
using FramePFX.Automation.ViewModels;

namespace FramePFX.PropertyEditing.Editors {
    public class AutomatableBoolEditorViewModel : AutomatablePropertyEditorViewModel<bool> {
        public AutomatableBoolEditorViewModel(Func<IAutomatableViewModel, bool> getter, Action<IAutomatableViewModel, bool> setter, AutomationKey automationKey) : base(automationKey, getter, setter) {
        }

        protected override void OnValueChanged(IReadOnlyList<IAutomatableViewModel> handlers, bool oldValue, bool value) {
            this.SetValuesAndHistory(value);
        }

        protected override void OnResetValue(IReadOnlyList<IAutomatableViewModel> handlers) {
            this.SetValuesAndHistory(((KeyDescriptorBoolean) this.AutomationKey.Descriptor).DefaultValue);
        }
    }
}