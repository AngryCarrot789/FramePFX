using SharpPadV2.Core.Shortcuts.Inputs;

namespace SharpPadV2.Core.Shortcuts.Dialogs {
    public interface IMouseDialogService {
        MouseStroke? ShowGetMouseStrokeDialog();
    }
}