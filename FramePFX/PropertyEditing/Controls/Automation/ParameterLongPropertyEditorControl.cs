using System;
using System.Windows;
using FramePFX.Editors.Automation.Params;
using FramePFX.PropertyEditing.Automation;

namespace FramePFX.PropertyEditing.Controls.Automation {
    public class ParameterLongPropertyEditorControl : BaseNumberParameterPropEditorControl {
        public new ParameterLongPropertyEditorSlot SlotModel => (ParameterLongPropertyEditorSlot) base.SlotControl.Model;

        public ParameterLongPropertyEditorControl() {

        }

        static ParameterLongPropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ParameterLongPropertyEditorControl), new FrameworkPropertyMetadata(typeof(ParameterLongPropertyEditorControl)));

        protected override void UpdateControlValue() {
            this.dragger.Value = this.SlotModel.Value;
        }

        protected override void UpdateModelValue() {
            this.SlotModel.Value = (long) Math.Round(this.dragger.Value);
        }

        protected override void OnConnected() {
            base.OnConnected();
            ParameterLongPropertyEditorSlot slot = this.SlotModel;
            ParameterDescriptorLong desc = slot.Parameter.Descriptor;
            this.dragger.Minimum = desc.Minimum;
            this.dragger.Maximum = desc.Maximum;

            DragStepProfile profile = slot.StepProfile;
            this.dragger.TinyChange = Math.Max(profile.TinyStep, 1.0);
            this.dragger.SmallChange = Math.Max(profile.SmallStep, 1.0);
            this.dragger.LargeChange = Math.Max(profile.NormalStep, 1.0);
            this.dragger.MassiveChange = Math.Max(profile.LargeStep, 1.0);
        }
    }
}