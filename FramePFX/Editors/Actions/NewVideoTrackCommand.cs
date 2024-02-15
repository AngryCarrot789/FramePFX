using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class NewVideoTrackCommand : Command {
        public override Task ExecuteAsync(CommandEventArgs e) {
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