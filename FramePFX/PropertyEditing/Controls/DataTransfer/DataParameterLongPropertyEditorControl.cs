using System;
using System.Windows;
using FramePFX.Editors.DataTransfer;
using FramePFX.PropertyEditing.Automation;
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.PropertyEditing.Controls.DataTransfer {
    public class DataParameterLongPropertyEditorControl : BaseNumberDataParamPropEditorControl {
        public new DataParameterLongPropertyEditorSlot SlotModel => (DataParameterLongPropertyEditorSlot) base.SlotControl.Model;

        public DataParameterLongPropertyEditorControl() {

        }

        static DataParameterLongPropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(DataParameterLongPropertyEditorControl), new FrameworkPropertyMetadata(typeof(DataParameterLongPropertyEditorControl)));

        protected override void UpdateControlValue() {
            this.dragger.Value = this.SlotModel.Value;
        }

        protected override void UpdateModelValue() {
            this.SlotModel.Value = (long) Math.Round(this.dragger.Value);
        }

        protected override void OnConnected() {
            base.OnConnected();
            DataParameterLongPropertyEditorSlot slot = this.SlotModel;
            DataParameterLong param = slot.DataParameter;
            this.dragger.Minimum = param.Minimum;
            this.dragger.Maximum = param.Maximum;

            DragStepProfile profile = slot.StepProfile;
            this.dragger.TinyChange = Math.Max(profile.TinyStep, 1.0);
            this.dragger.SmallChange = Math.Max(profile.SmallStep, 1.0);
            this.dragger.LargeChange = Math.Max(profile.NormalStep, 1.0);
            this.dragger.MassiveChange = Math.Max(profile.LargeStep, 1.0);
        }
    }
}