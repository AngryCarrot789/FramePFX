using FramePFX.Editors.Automation.Params;
using System;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Editors.Timelines.Tracks;
using OpenTK.Graphics.ES11;

namespace FramePFX.PropertyEditing.Standard {
    public delegate void ParameterPropertyEditorSlotEventHandler(ParameterPropertyEditorSlot slot);

    public abstract class ParameterPropertyEditorSlot : PropertyEditorSlot {
        private string displayName;

        protected IAutomatable SingleHandler => (IAutomatable) this.Handlers[0];

        public Parameter Parameter { get; }

        public string DisplayName {
            get => this.displayName;
            set {
                if (this.displayName == value)
                    return;
                this.displayName = value;
                this.DisplayNameChanged?.Invoke(this);
            }
        }

        public override bool IsSelectable => true;

        public event ParameterPropertyEditorSlotEventHandler DisplayNameChanged;
        public event ParameterPropertyEditorSlotEventHandler ValueChanged;

        protected ParameterPropertyEditorSlot(Parameter parameter, Type applicableType, string displayName = null) : base(applicableType) {
            this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            this.displayName = displayName ?? parameter.Key.ToString();
            this.IsSelectedChanged += this.OnIsSelectedChanged;
        }

        private void OnIsSelectedChanged(PropertyEditorSlot sender) {
            if (!this.IsSelected)
                return;

            foreach (IAutomatable automatable in this.Handlers) {
                if (automatable is Clip clip) {
                    clip.ActiveSequence = clip.AutomationData[this.Parameter];
                }
                else if (automatable is BaseEffect effect && effect.Owner is Clip effectClipOwner) {
                    effectClipOwner.ActiveSequence = effect.AutomationData[this.Parameter];
                }
            }
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            if (this.IsSingleHandler)
                this.SingleHandler.AutomationData[this.Parameter].ParameterChanged += this.OnValueForSingleHandlerChanged;
            this.QueryValueFromHandlers();
            this.OnValueChanged();
        }

        protected override void OnClearingHandlers() {
            base.OnClearingHandlers();
            if (this.IsSingleHandler)
                this.SingleHandler.AutomationData[this.Parameter].ParameterChanged -= this.OnValueForSingleHandlerChanged;
        }

        private void OnValueForSingleHandlerChanged(AutomationSequence sequence) {
            this.QueryValueFromHandlers();
            this.OnValueChanged();
        }

        public abstract void QueryValueFromHandlers();

        protected void OnValueChanged() {
            this.ValueChanged?.Invoke(this);
        }
    }
}
