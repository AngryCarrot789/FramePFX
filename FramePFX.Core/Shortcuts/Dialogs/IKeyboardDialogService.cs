using MCNBTViewer.Core.Shortcuts.Inputs;

namespace MCNBTViewer.Core.Shortcuts.Dialogs {
    public interface IKeyboardDialogService {
        KeyStroke? ShowGetKeyStrokeDialog();
    }
}