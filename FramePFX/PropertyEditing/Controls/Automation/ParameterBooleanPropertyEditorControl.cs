using System.Windows;
using System.Windows.Controls;
using FramePFX.PropertyEditing.Automation;

namespace FramePFX.PropertyEditing.Controls.Automation {
    public class ParameterBooleanPropertyEditorControl : BaseParameterPropertyEditorControl {
        private CheckBox valueCheckBox;
        protected bool IsUpdatingControl;

        public new ParameterBooleanPropertyEditorSlot SlotModel => (ParameterBooleanPropertyEditorSlot) base.SlotControl.Model;

        public ParameterBooleanPropertyEditorControl() {

        }

        static ParameterBooleanPropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ParameterBooleanPropertyEditorControl), new FrameworkPropertyMetadata(typeof(ParameterBooleanPropertyEditorControl)));

        protected void UpdateControlValue() {
            this.valueCheckBox.IsChecked = this.SlotModel.Value;
        }

        protected void UpdateModelValue() {
            this.SlotModel.Value = this.valueCheckBox.IsChecked ?? false;
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.valueCheckBox = this.GetTemplateChild<CheckBox>("PART_ValueCheckBox");

            RoutedEventHandler checkChanged = (s, e) => this.OnControlValueChanged();
            this.valueCheckBox.Checked += checkChanged;
            this.valueCheckBox.Unchecked += checkChanged;
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
            this.SlotModel.ValueChanged += this.SlotOnValueChanged;
        }

        protected override void OnDisconnected() {
            base.OnDisconnected();
            this.SlotModel.ValueChanged += this.SlotOnValueChanged;
        }

        private void SlotOnValueChanged(ParameterPropertyEditorSlot slot) {
            this.OnModelValueChanged();
        }
    }
}