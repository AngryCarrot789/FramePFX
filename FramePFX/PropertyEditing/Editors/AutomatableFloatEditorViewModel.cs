using System;
using System.Collections.Generic;
using FramePFX.Automation.Keys;
using FramePFX.Automation.ViewModels;

namespace FramePFX.PropertyEditing.Editors {
    public class AutomatableFloatEditorViewModel : AutomatablePropertyEditorViewModel<float> {
        public float Min { get; }

        public float Max { get; }

        public AutomatableFloatEditorViewModel(Type applicableType, AutomationKey automationKey, Func<IAutomatableViewModel, float> getter, Action<IAutomatableViewModel, float> setter) :
            base(applicableType, automationKey, getter, setter) {
            KeyDescriptorFloat desc = (KeyDescriptorFloat) automationKey.Descriptor;
            this.Min = float.IsInfinity(desc.Minimum) ? float.MinValue : desc.Minimum;
            this.Max = float.IsInfinity(desc.Maximum) ? float.MaxValue : desc.Maximum;
        }

        public static AutomatableFloatEditorViewModel NewInstance<TOwner>(AutomationKey automationKey, Func<TOwner, float> getter, Action<TOwner, float> setter) where TOwner : IAutomatableViewModel {
            return new AutomatableFloatEditorViewModel(typeof(TOwner), automationKey, (o) => getter((TOwner) o), (o, v) => setter((TOwner) o, v));
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

        protected override void InsertKeyFrame(IAutomatableViewModel handler, long frame) {
            handler.AutomationData[this.AutomationKey].GetActiveKeyFrameOrCreateNew(frame).SetFloatValue(this.Getter(handler));
        }
    }
}