using System.Windows;
using System.Windows.Controls;
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.PropertyEditing.Controls.DataTransfer {
    public class DataParameterStringPropertyEditorControl : BaseDataParameterPropertyEditorControl {
        protected TextBox textBox;

        public new DataParameterStringPropertyEditorSlot SlotModel => (DataParameterStringPropertyEditorSlot) base.SlotControl.Model;

        public DataParameterStringPropertyEditorControl() {

        }

        static DataParameterStringPropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(DataParameterStringPropertyEditorControl), new FrameworkPropertyMetadata(typeof(DataParameterStringPropertyEditorControl)));

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.textBox = this.GetTemplateChild<TextBox>("PART_TextBox");
            this.textBox.TextChanged += (sender, args) => this.OnControlValueChanged();
        }

        protected override void UpdateControlValue() {
            this.textBox.Text = this.SlotModel.Value;
        }

        protected override void UpdateModelValue() {
            this.SlotModel.Value = this.textBox.Text;
        }

        protected override void OnCanEditValueChanged(bool canEdit) {
            this.textBox.IsEnabled = canEdit;
        }
    }
}