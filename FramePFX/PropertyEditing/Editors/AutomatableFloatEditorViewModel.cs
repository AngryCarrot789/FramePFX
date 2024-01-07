using System;
using System.Collections.Generic;
using FramePFX.Automation.Keys;
using FramePFX.Automation.ViewModels;
using FramePFX.Utils;

namespace FramePFX.PropertyEditing.Editors {
    public class AutomatableFloatEditorViewModel : AutomatablePropertyEditorViewModel<float> {
        public float Min { get; }

        public float Max { get; }

        public AutomatableFloatEditorViewModel(Type targetType, string propertyName, AutomationKey automationKey) : base(targetType, propertyName, automationKey) {
            KeyDescriptorFloat desc = (KeyDescriptorFloat) automationKey.Descriptor;
            this.Min = float.IsInfinity(desc.Minimum) ? float.MinValue : desc.Minimum;
            this.Max = float.IsInfinity(desc.Maximum) ? float.MaxValue : desc.Maximum;
        }

        public static AutomatableFloatEditorViewModel NewInstance<TOwner>(string propertyName, AutomationKey automationKey) where TOwner : IAutomatableViewModel {
            return new AutomatableFloatEditorViewModel(typeof(TOwner), propertyName, automationKey);
        }

        protected override void OnValueChanged(IReadOnlyList<IAutomatableViewModel> handlers, float oldValue, float value, bool isIncrementOperation) {
            if (isIncrementOperation) {
                float change = value - oldValue;
                for (int i = 0, count = handlers.Count; i < count; i++) {
                    this.SetValueAndHistory(i, this.Getter(handlers[i]) + change);
                }
            }
            else {
                this.SetValuesAndHistory(value);
            }
        }

        protected override void OnResetValue(IReadOnlyList<IAutomatableViewModel> handlers) {
            this.SetValuesAndHistory(((KeyDescriptorFloat) this.AutomationKey.Descriptor).DefaultValue);
        }

        protected override void InsertKeyFrame(IAutomatableViewModel handler, long frame) {
            handler.AutomationData[this.AutomationKey].GetActiveKeyFrameOrCreateNew(frame).SetFloatValue(this.Getter(handler));
        }
    }
}