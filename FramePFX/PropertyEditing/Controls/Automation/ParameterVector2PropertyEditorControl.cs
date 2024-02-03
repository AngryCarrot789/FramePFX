using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Controls.Dragger;
using FramePFX.PropertyEditing.Automation;

namespace FramePFX.PropertyEditing.Controls.Automation {
    public class ParameterVector2PropertyEditorControl : BaseParameterPropertyEditorControl {
        protected NumberDragger draggerX;
        protected NumberDragger draggerY;
        protected bool IsUpdatingControl;

        public new ParameterVector2PropertyEditorSlot SlotModel => (ParameterVector2PropertyEditorSlot) base.SlotControl.Model;

        public ParameterVector2PropertyEditorControl() {

        }

        static ParameterVector2PropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ParameterVector2PropertyEditorControl), new FrameworkPropertyMetadata(typeof(ParameterVector2PropertyEditorControl)));

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.draggerX = this.GetTemplateChild<NumberDragger>("PART_DraggerX");
            this.draggerY = this.GetTemplateChild<NumberDragger>("PART_DraggerY");
            this.draggerX.ValueChanged += (sender, args) => this.OnControlValueChanged();
            this.draggerY.ValueChanged += (sender, args) => this.OnControlValueChanged();
        }

        protected void UpdateControlValue() {
            Vector2 value = this.SlotModel.Value;
            this.draggerX.Value = value.X;
            this.draggerY.Value = value.Y;
        }

        protected void UpdateModelValue() {
            this.SlotModel.Value = new Vector2((float) this.draggerX.Value, (float) this.draggerY.Value);
        }

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

        protected override void OnConnected() {
            base.OnConnected();
            ParameterVector2PropertyEditorSlot slot = this.SlotModel;
            slot.ValueChanged += this.OnSlotValueChanged;

            ParameterDescriptorVector2 desc = slot.Parameter.Descriptor;
            this.draggerX.Minimum = desc.Minimum.X;
            this.draggerX.Maximum = desc.Maximum.X;
            this.draggerY.Minimum = desc.Minimum.Y;
            this.draggerY.Maximum = desc.Maximum.Y;

            DragStepProfile profile = slot.StepProfile;
            this.draggerX.TinyChange = this.draggerY.TinyChange = profile.TinyStep;
            this.draggerX.SmallChange = this.draggerY.SmallChange = profile.SmallStep;
            this.draggerX.LargeChange = this.draggerY.LargeChange = profile.NormalStep;
            this.draggerX.MassiveChange = this.draggerY.MassiveChange = profile.LargeStep;
        }

        protected override void OnDisconnected() {
            base.OnDisconnected();
            ParameterVector2PropertyEditorSlot slot = this.SlotModel;
            slot.ValueChanged -= this.OnSlotValueChanged;
        }

        private void OnSlotValueChanged(ParameterPropertyEditorSlot slot) {
            this.OnModelValueChanged();
        }
    }
}