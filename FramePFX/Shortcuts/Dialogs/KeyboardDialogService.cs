using FramePFX.Core;
using FramePFX.Core.Shortcuts.Dialogs;
using FramePFX.Core.Shortcuts.Inputs;

namespace FramePFX.Shortcuts.Dialogs {
    [Service(typeof(IKeyboardDialogService))]
    public class KeyboardDialogService : IKeyboardDialogService {
        public KeyStroke? ShowGetKeyStrokeDialog() {
            KeyStrokeInputWindow window = new KeyStrokeInputWindow();
            if (window.ShowDialog() != true || window.Stroke.Equals(default)) {
                return null;
            }
            else {
                return window.Stroke;
            }
        }
    }
}