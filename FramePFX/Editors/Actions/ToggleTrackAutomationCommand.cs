using FramePFX.Interactivity.DataContexts;
using FramePFX.CommandSystem;

namespace FramePFX.Editors.Actions {
    public class ToggleTrackAutomationCommand : Command {
        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.VideoEditorKey.TryGetContext(e.DataContext, out VideoEditor editor))
                return;
            editor.ShowTrackAutomation = !editor.ShowTrackAutomation;
        }
    }
}
