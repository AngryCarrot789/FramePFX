using FramePFX.Core.Shortcuts.Inputs;

namespace FramePFX.Core.Shortcuts.Dialogs
{
    public interface IKeyboardDialogService
    {
        KeyStroke? ShowGetKeyStrokeDialog();
    }
}