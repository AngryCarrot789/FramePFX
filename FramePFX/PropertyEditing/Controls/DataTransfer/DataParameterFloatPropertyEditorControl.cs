using System.Windows;
using FramePFX.Editors.DataTransfer;
using FramePFX.PropertyEditing.Automation;
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.PropertyEditing.Controls.DataTransfer {
    public class DataParameterFloatPropertyEditorControl : BaseNumberDataParamPropEditorControl {
        public new DataParameterFloatPropertyEditorSlot SlotModel => (DataParameterFloatPropertyEditorSlot) base.SlotControl.Model;

        public DataParameterFloatPropertyEditorControl() {

        }

        static DataParameterFloatPropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(DataParameterFloatPropertyEditorControl), new FrameworkPropertyMetadata(typeof(DataParameterFloatPropertyEditorControl)));

        protected override void UpdateControlValue() {
            this.dragger.Value = this.SlotModel.Value;
        }

        protected override void UpdateModelValue() {
            this.SlotModel.Value = (float) this.dragger.Value;
        }

        protected override void OnConnected() {
            base.OnConnected();
            DataParameterFloatPropertyEditorSlot slot = this.SlotModel;
            DataParameterFloat param = slot.DataParameter;
            this.dragger.Minimum = param.Minimum;
            this.dragger.Maximum = param.Maximum;

            DragStepProfile profile = slot.StepProfile;
            this.dragger.TinyChange = profile.TinyStep;
            this.dragger.SmallChange = profile.SmallStep;
            this.dragger.LargeChange = profile.NormalStep;
            this.dragger.MassiveChange = profile.LargeStep;
        }
    }
}