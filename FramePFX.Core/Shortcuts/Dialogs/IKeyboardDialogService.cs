using FrameControlEx.Core.Shortcuts.Inputs;

namespace FrameControlEx.Core.Shortcuts.Dialogs {
    public interface IKeyboardDialogService {
        KeyStroke? ShowGetKeyStrokeDialog();
    }
}