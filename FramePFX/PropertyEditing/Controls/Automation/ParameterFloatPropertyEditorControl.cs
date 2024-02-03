using System.Windows;
using FramePFX.Editors.Automation.Params;
using FramePFX.PropertyEditing.Automation;

namespace FramePFX.PropertyEditing.Controls.Automation {
    public class ParameterFloatPropertyEditorControl : BaseSliderParameterPropertyEditorControl {
        public new ParameterFloatPropertyEditorSlot SlotModel => (ParameterFloatPropertyEditorSlot) base.SlotControl.Model;

        public ParameterFloatPropertyEditorControl() {

        }

        static ParameterFloatPropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ParameterFloatPropertyEditorControl), new FrameworkPropertyMetadata(typeof(ParameterFloatPropertyEditorControl)));

        protected override void UpdateControlValue() {
            this.dragger.Value = this.SlotModel.Value;
        }

        protected override void UpdateModelValue() {
            this.SlotModel.Value = (float) this.dragger.Value;
        }

        protected override void OnConnected() {
            base.OnConnected();
            ParameterFloatPropertyEditorSlot slot = this.SlotModel;
            ParameterDescriptorFloat desc = slot.Parameter.Descriptor;
            this.dragger.Minimum = desc.Minimum;
            this.dragger.Maximum = desc.Maximum;

            DragStepProfile profile = slot.StepProfile;
            this.dragger.TinyChange = profile.TinyStep;
            this.dragger.SmallChange = profile.SmallStep;
            this.dragger.LargeChange = profile.NormalStep;
            this.dragger.MassiveChange = profile.LargeStep;
        }
    }
}