using System;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Controls.Automation;
using FramePFX.Editors.PropertyEditors.Effects.Motion;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.PropertyEditing;
using FramePFX.Utils;

namespace FramePFX.Editors.PropertyEditors.Standard {
    public delegate void AutomatedFloatValueChangedEventHandler(AutomatedFloatPropertyEditorSlot slot);

    public class AutomatedFloatPropertyEditorSlot : PropertyEditorSlot {
        private float value;

        public IAutomatable SingleHandler => (IAutomatable) this.Handlers[0];

        public ParameterFloat Parameter { get; }

        public float Value {
            get => this.value;
            set {
                float oldVal = this.value;
                this.value = value;
                bool useAddition = this.Handlers.Count > 1;
                float change = value - oldVal;
                for (int i = 0, c = this.Handlers.Count; i < c; i++) {
                    IAutomatable obj = (IAutomatable) this.Handlers[i];
                    float newClipValue = this.Parameter.Descriptor.Clamp(useAddition ? (this.Parameter.GetEffectiveValue(obj) + change) : value);
                    AutomatedUtils.SetDefaultKeyFrameOrAddNew(obj, this.Parameter, newClipValue);
                }

                this.ValueChanged?.Invoke(this);
            }
        }

        public string DisplayName { get; }

        public DragStepProfile StepProfile { get; }

        public override bool IsSelectable => true;

        public event AutomatedFloatValueChangedEventHandler ValueChanged;

        public AutomatedFloatPropertyEditorSlot(ParameterFloat parameter, Type applicableType, string displayName, DragStepProfile stepProfile) : base(applicableType) {
            this.Parameter = parameter;
            this.DisplayName = displayName;
            this.StepProfile = stepProfile;
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            if (this.IsSingleHandler)
                this.SingleHandler.AutomationData[this.Parameter].ParameterChanged += this.OnValueForSingleHandlerChanged;
            this.RequeryOpacityFromHandlers();
        }

        protected override void OnClearingHandlers() {
            base.OnClearingHandlers();
            if (this.IsSingleHandler)
                this.SingleHandler.AutomationData[this.Parameter].ParameterChanged -= this.OnValueForSingleHandlerChanged;
        }

        public void RequeryOpacityFromHandlers() {
            this.value = GetEqualValue(this.Handlers, (x) => this.Parameter.GetEffectiveValue((IAutomatable) x), out float d) ? d : default;
            this.ValueChanged?.Invoke(this);
        }

        private void OnValueForSingleHandlerChanged(AutomationSequence sequence) {
            this.value = this.Parameter.GetEffectiveValue(this.SingleHandler);
            this.ValueChanged?.Invoke(this);
        }
    }
}