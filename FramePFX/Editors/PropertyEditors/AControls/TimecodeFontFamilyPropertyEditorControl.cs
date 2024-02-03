using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.PropertyEditors.Clips;
using FramePFX.PropertyEditing.Controls;

namespace FramePFX.Editors.PropertyEditors.AControls {
    public class TimecodeFontFamilyPropertyEditorControl : BasePropEditControlContent {
        public TimecodeFontFamilyPropertyEditorSlot SlotModel => (TimecodeFontFamilyPropertyEditorSlot) base.SlotControl.Model;

        private TextBox fontFamilyTextBox;

        private readonly GetSetAutoPropertyBinder<TimecodeFontFamilyPropertyEditorSlot> fontFamilyBinder = new GetSetAutoPropertyBinder<TimecodeFontFamilyPropertyEditorSlot>(TextBox.TextProperty, nameof(TimecodeFontFamilyPropertyEditorSlot.FontFamilyChanged), binder => binder.Model.FontFamily, (binder, v) => binder.Model.SetValue((string) v));

        public TimecodeFontFamilyPropertyEditorControl() {
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.fontFamilyTextBox = this.GetTemplateChild<TextBox>("PART_TextBox");
            this.fontFamilyTextBox.TextChanged += (sender, args) => this.fontFamilyBinder.OnControlValueChanged();
        }

        static TimecodeFontFamilyPropertyEditorControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimecodeFontFamilyPropertyEditorControl), new FrameworkPropertyMetadata(typeof(TimecodeFontFamilyPropertyEditorControl)));
        }

        protected override void OnConnected() {
            this.fontFamilyBinder.Attach(this.fontFamilyTextBox, this.SlotModel);
        }

        protected override void OnDisconnected() {
            this.fontFamilyBinder.Detatch();
        }
    }
}