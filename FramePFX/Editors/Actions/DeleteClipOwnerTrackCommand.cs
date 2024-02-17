using FramePFX.CommandSystem;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class DeleteClipOwnerTrackCommand : Command {
        public override void Execute(CommandEventArgs e) {
            if (DataKeys.ClipKey.TryGetContext(e.DataContext, out Clip clip)) {
                clip.Timeline?.DeleteTrack(clip.Track);
            }
        }
    }
}