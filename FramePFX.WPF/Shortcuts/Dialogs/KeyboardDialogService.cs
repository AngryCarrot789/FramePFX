using FramePFX.Shortcuts.Dialogs;
using FramePFX.Shortcuts.Inputs;

namespace FramePFX.WPF.Shortcuts.Dialogs
{
    [ServiceImplementation(typeof(IKeyboardDialogService))]
    public class KeyboardDialogService : IKeyboardDialogService
    {
        public KeyStroke? ShowGetKeyStrokeDialog()
        {
            KeyStrokeInputWindow window = new KeyStrokeInputWindow();
            if (window.ShowDialog() != true || window.Stroke.Equals(default))
            {
                return null;
            }
            else
            {
                return window.Stroke;
            }
        }
    }
}