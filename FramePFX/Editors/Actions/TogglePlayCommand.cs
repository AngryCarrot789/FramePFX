using FramePFX.CommandSystem;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class TogglePlayCommand : Command {
        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.VideoEditorKey.TryGetContext(e.DataContext, out VideoEditor editor))
                return;

            if (editor.Playback.PlayState == PlayState.Play) {
                editor.Playback.Pause();
            }
            else {
                editor.Playback.Play();
            }
        }
    }
}