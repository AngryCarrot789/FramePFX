using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class DeleteClipOwnerTrackAction : AnAction {
        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (e.DataContext.TryGetContext(DataKeys.ClipKey, out Clip clip)) {
                clip.Timeline?.DeleteTrack(clip.Track);
            }

            return Task.CompletedTask;
        }
    }
}