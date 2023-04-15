using System.Windows.Input;
using FramePFX.Core.Shortcuts.Inputs;
using FramePFX.Core.Views.Dialogs;
using FramePFX.Shortcuts.Views;
using FramePFX.Views;

namespace FramePFX.Shortcuts.Dialogs {
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
