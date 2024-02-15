using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class DeleteClipOwnerTrackCommand : Command {
        public override Task ExecuteAsync(CommandEventArgs e) {
            if (e.DataContext.TryGetContext(DataKeys.ClipKey, out Clip clip)) {
                clip.Timeline?.DeleteTrack(clip.Track);
            }

            return Task.CompletedTask;
        }
    }
}