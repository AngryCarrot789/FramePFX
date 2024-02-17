using FramePFX.CommandSystem;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class NewVideoTrackCommand : Command {
        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.TimelineKey.TryGetContext(e.DataContext, out Timeline timeline)) {
                return;
            }

            VideoTrack track = new VideoTrack() {
                DisplayName = "New Video Track"
            };

            timeline.AddTrack(track);
        }
    }
}