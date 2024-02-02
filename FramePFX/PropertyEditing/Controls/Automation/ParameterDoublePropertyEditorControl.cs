using System.Windows;
using FramePFX.Editors.Automation.Params;
using FramePFX.PropertyEditing.Automation;

namespace FramePFX.PropertyEditing.Controls.Automation {
    public class ParameterDoublePropertyEditorControl : BaseSliderParameterPropertyEditorControl {
        public new ParameterDoublePropertyEditorSlot SlotModel => (ParameterDoublePropertyEditorSlot) base.SlotControl.Model;

        public ParameterDoublePropertyEditorControl() {

        }
        static ParameterDoublePropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ParameterDoublePropertyEditorControl), new FrameworkPropertyMetadata(typeof(ParameterDoublePropertyEditorControl)));

        protected override void UpdateControlValue() {
            this.dragger.Value = this.SlotModel.Value;
        }

        protected override void UpdateModelValue() {
            this.SlotModel.Value = this.dragger.Value;
        }

        protected override void OnConnectedOverride() {
            ParameterDoublePropertyEditorSlot slot = this.SlotModel;
            ParameterDescriptorDouble desc = slot.Parameter.Descriptor;
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