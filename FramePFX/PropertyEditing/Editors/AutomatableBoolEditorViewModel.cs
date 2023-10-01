using System;
using System.Collections.Generic;
using FramePFX.Automation.Keys;
using FramePFX.Automation.ViewModels;

namespace FramePFX.PropertyEditing.Editors
{
    public class AutomatableBoolEditorViewModel : AutomatablePropertyEditorViewModel<bool>
    {
        public AutomatableBoolEditorViewModel(Type applicableType, AutomationKey automationKey, Func<IAutomatableViewModel, bool> getter, Action<IAutomatableViewModel, bool> setter) : base(applicableType, automationKey, getter, setter)
        {
        }

        protected override void OnValueChanged(IReadOnlyList<IAutomatableViewModel> handlers, bool oldValue, bool value)
        {
            this.SetValuesAndHistory(value);
        }

        protected override void OnResetValue(IReadOnlyList<IAutomatableViewModel> handlers)
        {
            this.SetValuesAndHistory(((KeyDescriptorBoolean) this.AutomationKey.Descriptor).DefaultValue);
        }

        protected override void InsertKeyFrame(IAutomatableViewModel handler, long frame)
        {
            handler.AutomationData[this.AutomationKey].GetActiveKeyFrameOrCreateNew(frame).SetBooleanValue(this.Getter(handler));
        }
    }
}