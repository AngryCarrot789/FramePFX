using System.Windows;
using System.Windows.Controls;
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.PropertyEditing.Controls.DataTransfer {
    public class DataParameterBooleanPropertyEditorControl : BaseDataParameterPropertyEditorControl {
        protected CheckBox checkBox;

        public new DataParameterBooleanPropertyEditorSlot SlotModel => (DataParameterBooleanPropertyEditorSlot) base.SlotControl.Model;

        public DataParameterBooleanPropertyEditorControl() {

        }

        static DataParameterBooleanPropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(DataParameterBooleanPropertyEditorControl), new FrameworkPropertyMetadata(typeof(DataParameterBooleanPropertyEditorControl)));

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.checkBox = this.GetTemplateChild<CheckBox>("PART_CheckBox");
            RoutedEventHandler handler = (s, e) => this.OnControlValueChanged();
            this.checkBox.Checked += handler;
            this.checkBox.Unchecked += handler;
        }

        protected override void UpdateControlValue() {
            this.checkBox.IsChecked = this.SlotModel.Value;
        }

        protected override void UpdateModelValue() {
            this.SlotModel.Value = this.checkBox.IsChecked ?? false;
        }

        protected override void OnCanEditValueChanged(bool canEdit) {
            this.checkBox.IsEnabled = canEdit;
        }
    }
}