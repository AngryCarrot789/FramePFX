using System.Windows;
using FramePFX.Editors.DataTransfer;
using FramePFX.PropertyEditing.Automation;
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.PropertyEditing.Controls.DataTransfer {
    public class DataParameterDoublePropertyEditorControl : BaseNumberDataParamPropEditorControl {
        public new DataParameterDoublePropertyEditorSlot SlotModel => (DataParameterDoublePropertyEditorSlot) base.SlotControl.Model;

        public DataParameterDoublePropertyEditorControl() {

        }
        static DataParameterDoublePropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(DataParameterDoublePropertyEditorControl), new FrameworkPropertyMetadata(typeof(DataParameterDoublePropertyEditorControl)));

        protected override void UpdateControlValue() {
            this.dragger.Value = this.SlotModel.Value;
        }

        protected override void UpdateModelValue() {
            this.SlotModel.Value = this.dragger.Value;
        }

        protected override void OnConnected() {
            base.OnConnected();
            DataParameterDoublePropertyEditorSlot slot = this.SlotModel;
            DataParameterDouble param = slot.DataParameter;
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