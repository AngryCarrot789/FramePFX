using FramePFX.Interactivity.DataContexts;
using FramePFX.CommandSystem;

namespace FramePFX.Editors.Actions {
    public class ToggleClipAutomationCommand : Command {
        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.VideoEditorKey.TryGetContext(e.DataContext, out VideoEditor editor))
                return;
            editor.ShowClipAutomation = !editor.ShowClipAutomation;
        }
    }
}
