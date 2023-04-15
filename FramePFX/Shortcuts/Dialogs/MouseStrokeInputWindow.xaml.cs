using System.Windows.Input;
using SharpPadV2.Core.Shortcuts.Inputs;
using SharpPadV2.Core.Views.Dialogs;
using SharpPadV2.Shortcuts.Views;
using SharpPadV2.Views;

namespace SharpPadV2.Shortcuts.Dialogs {
    /// <summary>
    /// Interaction logic for MouseStrokeInputWindow.xaml
    /// </summary>
    public partial class MouseStrokeInputWindow : BaseDialog {
        public MouseStroke Stroke { get; set; }

        public MouseStrokeInputWindow() {
            this.InitializeComponent();
            this.DataContext = new BaseConfirmableDialogViewModel(this);
            this.InputBox.Text = "";
        }

        private void InputBox_MouseDown(object sender, MouseButtonEventArgs e) {
            MouseStroke stroke = ShortcutUtils.GetMouseStrokeForEvent(e);
            this.Stroke = stroke;
            this.InputBox.Text = MouseStrokeRepresentationConverter.ToStringFunction(stroke.MouseButton, stroke.Modifiers, stroke.ClickCount, stroke.WheelDelta);
        }

        private void InputBox_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (ShortcutUtils.GetMouseStrokeForEvent(e, out MouseStroke stroke)) {
                this.Stroke = stroke;
                this.InputBox.Text = MouseStrokeRepresentationConverter.ToStringFunction(stroke.MouseButton, stroke.Modifiers, stroke.ClickCount, stroke.WheelDelta);
            }
        }
    }
}
