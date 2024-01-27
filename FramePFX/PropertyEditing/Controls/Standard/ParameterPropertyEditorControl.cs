using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Controls.Dragger;
using FramePFX.PropertyEditing.Automation;
using FramePFX.PropertyEditing.Standard;

namespace FramePFX.PropertyEditing.Controls.Standard {
    public abstract class ParameterPropertyEditorControl : BasePropEditControlContent {
        protected NumberDragger dragger;
        protected TextBlock displayName;
        protected KeyFrameToolsControl keyFrameTools;
        protected bool IsUpdatingControl;
        protected IAutomatable singleHandler;

        public ParameterPropertyEditorSlot SlotModel => (ParameterPropertyEditorSlot) base.SlotControl.Model;

        protected ParameterPropertyEditorControl() {

        }

        static ParameterPropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ParameterPropertyEditorControl), new FrameworkPropertyMetadata(typeof(ParameterPropertyEditorControl)));

        protected abstract void UpdateControlValue();

        protected abstract void UpdateModelValue();

        private void OnModelValueChanged() {
            if (this.SlotModel != null) {
                this.IsUpdatingControl = true;
                try {
                    this.UpdateControlValue();
                }
                finally {
                    this.IsUpdatingControl = false;
                }
            }
        }

        private void OnControlValueChanged() {
            if (!this.IsUpdatingControl && this.SlotModel != null) {
                this.UpdateModelValue();
            }
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.dragger = this.GetTemplateChild<NumberDragger>("PART_DraggerX");
            this.displayName = this.GetTemplateChild<TextBlock>("PART_DisplayName");
            this.keyFrameTools = this.GetTemplateChild<KeyFrameToolsControl>("PART_KeyFrameTools");
            this.dragger.ValueChanged += (sender, args) => this.OnControlValueChanged();
        }

        protected sealed override void OnConnected() {
            ParameterPropertyEditorSlot slot = this.SlotModel;
            slot.ValueChanged += this.OnSlotValueChanged;
            slot.HandlersLoaded += this.OnHandlersChanged;
            slot.HandlersCleared += this.OnHandlersChanged;
            slot.DisplayNameChanged += this.OnSlotDisplayNameChanged;
            this.displayName.Text = slot.DisplayName;
            this.OnConnectedOverride();
            this.OnHandlerListChanged(true);
        }

        protected abstract void OnConnectedOverride();

        protected override void OnDisconnected() {
            ParameterPropertyEditorSlot slot = this.SlotModel;
            slot.ValueChanged -= this.OnSlotValueChanged;
            slot.HandlersLoaded -= this.OnHandlersChanged;
            slot.HandlersCleared -= this.OnHandlersChanged;
            this.OnHandlerListChanged(false);
        }

        private void OnSlotValueChanged(ParameterPropertyEditorSlot slot) {
            this.OnModelValueChanged();
        }

        private void OnHandlerListChanged(bool connect) {
            ParameterPropertyEditorSlot slot = this.SlotModel;
            if (connect && slot != null && slot.Handlers.Count == 1) {
                this.singleHandler = (IAutomatable) slot.Handlers[0];
                this.keyFrameTools.Visibility = Visibility.Visible;
                this.keyFrameTools.AutomationSequence = this.singleHandler.AutomationData[slot.Parameter];
            }
            else {
                this.keyFrameTools.Visibility = Visibility.Collapsed;
                this.keyFrameTools.AutomationSequence = null;
            }
        }

        private void OnHandlersChanged(PropertyEditorSlot sender) {
            this.OnHandlerListChanged(true);
        }

        private void OnSlotDisplayNameChanged(ParameterPropertyEditorSlot slot) {
            if (this.displayName != null)
                this.displayName.Text = slot.DisplayName;
        }
    }
}