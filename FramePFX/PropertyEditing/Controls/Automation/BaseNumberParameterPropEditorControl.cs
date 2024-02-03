using System.Windows;
using FramePFX.Editors.Controls.Dragger;
using FramePFX.PropertyEditing.Automation;

namespace FramePFX.PropertyEditing.Controls.Automation {
    public abstract class BaseNumberParameterPropEditorControl : BaseParameterPropertyEditorControl {
        protected NumberDragger dragger;
        protected bool IsUpdatingControl;

        protected BaseNumberParameterPropEditorControl() {

        }

        static BaseNumberParameterPropEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseNumberParameterPropEditorControl), new FrameworkPropertyMetadata(typeof(BaseNumberParameterPropEditorControl)));

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
            this.dragger.ValueChanged += (sender, args) => this.OnControlValueChanged();
        }

        protected override void OnConnected() {
            base.OnConnected();
            ParameterPropertyEditorSlot slot = (ParameterPropertyEditorSlot) this.SlotModel;
            slot.ValueChanged += this.OnSlotValueChanged;
        }

        protected override void OnDisconnected() {
            base.OnDisconnected();
            ParameterPropertyEditorSlot slot = (ParameterPropertyEditorSlot) this.SlotModel;
            slot.ValueChanged -= this.OnSlotValueChanged;
        }

        private void OnSlotValueChanged(ParameterPropertyEditorSlot slot) {
            this.OnModelValueChanged();
        }
    }
}