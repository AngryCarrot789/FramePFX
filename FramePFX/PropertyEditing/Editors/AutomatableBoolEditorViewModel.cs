using System;
using System.Collections.Generic;
using FramePFX.Automation.Keys;
using FramePFX.Automation.ViewModels;

namespace FramePFX.PropertyEditing.Editors {
    public class AutomatableBoolEditorViewModel : AutomatablePropertyEditorViewModel<bool> {
        public AutomatableBoolEditorViewModel(Type targetType, string propertyName, AutomationKey automationKey) : base(targetType, propertyName, automationKey) {
        }

        protected override void OnValueChanged(IReadOnlyList<IAutomatableViewModel> handlers, bool oldValue, bool value, bool isIncrementOperation) {
            this.SetValuesAndHistory(value);
        }

        protected override void OnResetValue(IReadOnlyList<IAutomatableViewModel> handlers) {
            this.SetValuesAndHistory(((KeyDescriptorBoolean) this.AutomationKey.Descriptor).DefaultValue);
        }

        protected override void InsertKeyFrame(IAutomatableViewModel handler, long frame) {
            handler.AutomationData[this.AutomationKey].GetActiveKeyFrameOrCreateNew(frame).SetBooleanValue(this.Getter(handler));
        }
    }
}