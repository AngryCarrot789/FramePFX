using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class NewVideoTrackAction : AnAction {
        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(DataKeys.TimelineKey, out Timeline timeline)) {
                return Task.CompletedTask;
            }

            VideoTrack track = new VideoTrack() {
                DisplayName = "New Video Track"
            };

            timeline.AddTrack(track);
            return Task.CompletedTask;
        }
    }
}