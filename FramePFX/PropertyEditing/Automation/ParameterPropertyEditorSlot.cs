using System;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;

namespace FramePFX.PropertyEditing.Automation {
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
            bool selected = this.IsSelected;
            foreach (IAutomatable automatable in this.Handlers) {
                if (automatable is Clip clip) {
                    clip.ActiveSequence = selected ? clip.AutomationData[this.Parameter] : null;
                }
                else if (automatable is BaseEffect effect && effect.Owner is Clip effectClipOwner) {
                    effectClipOwner.ActiveSequence = selected ? effect.AutomationData[this.Parameter] : null;
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
