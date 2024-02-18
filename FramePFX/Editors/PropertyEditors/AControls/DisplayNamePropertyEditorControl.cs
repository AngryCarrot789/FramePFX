using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.PropertyEditors.Clips;
using FramePFX.PropertyEditing.Controls;

namespace FramePFX.Editors.PropertyEditors.AControls {
    public class DisplayNamePropertyEditorControl : BasePropEditControlContent {
        public DisplayNamePropertyEditorSlot SlotModel => (DisplayNamePropertyEditorSlot) base.SlotControl.Model;

        private TextBox displayNameBox;

        private readonly GetSetAutoEventPropertyBinder<DisplayNamePropertyEditorSlot> displayNameBinder = new GetSetAutoEventPropertyBinder<DisplayNamePropertyEditorSlot>(TextBox.TextProperty, nameof(DisplayNamePropertyEditorSlot.DisplayNameChanged), binder => binder.Model.DisplayName, (binder, v) => binder.Model.SetValue((string) v));

        public DisplayNamePropertyEditorControl() {
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.displayNameBox = this.GetTemplateChild<TextBox>("PART_TextBox");
        }

        static DisplayNamePropertyEditorControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DisplayNamePropertyEditorControl), new FrameworkPropertyMetadata(typeof(DisplayNamePropertyEditorControl)));
        }

        protected override void OnConnected() {
            this.displayNameBinder.Attach(this.displayNameBox, this.SlotModel);
        }

        protected override void OnDisconnected() {
            this.displayNameBinder.Detatch();
        }
    }
}