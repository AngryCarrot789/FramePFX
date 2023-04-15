using SharpPadV2.Core.Shortcuts.Dialogs;
using SharpPadV2.Core.Shortcuts.Inputs;

namespace SharpPadV2.Shortcuts.Dialogs {
    public class MouseDialogService : IMouseDialogService {
        public MouseStroke? ShowGetMouseStrokeDialog() {
            MouseStrokeInputWindow window = new MouseStrokeInputWindow();
            if (window.ShowDialog() != true || window.Stroke.Equals(default)) {
                return null;
            }
            else {
                return window.Stroke;
            }
        }
    }
}