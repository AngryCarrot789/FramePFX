using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class TogglePlayCommand : Command {
        public override Task ExecuteAsync(CommandEventArgs e) {
            if (!e.DataContext.TryGetContext(DataKeys.VideoEditorKey, out VideoEditor editor))
                return Task.CompletedTask;

            if (editor.Playback.PlayState == PlayState.Play) {
                editor.Playback.Pause();
            }
            else {
                editor.Playback.Play();
            }

            return Task.CompletedTask;
        }
    }
}