using FramePFX.Core.Shortcuts.Dialogs;
using FramePFX.Core.Shortcuts.Inputs;

namespace FramePFX.Shortcuts.Dialogs {
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