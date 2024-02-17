using FramePFX.CommandSystem;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class RenameResourceCommand : Command {
        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.ResourceObjectKey.TryGetContext(e.DataContext, out BaseResource resource)) {
                return;
            }

            if (IoC.UserInputService.ShowSingleInputDialog("Rename resource item", "Input a new name for this resource", resource.DisplayName) is string newDisplayName) {
                resource.DisplayName = newDisplayName;
            }
        }
    }

    public class RenameClipCommand : Command {
        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.ClipKey.TryGetContext(e.DataContext, out Clip clip)) {
                return;
            }

            if (IoC.UserInputService.ShowSingleInputDialog("Rename clip", "Input a new name for this clip", clip.DisplayName) is string newDisplayName) {
                clip.DisplayName = newDisplayName;
            }
        }
    }

    public class RenameTrackCommand : Command {
        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.TrackKey.TryGetContext(e.DataContext, out Track track)) {
                return;
            }

            if (IoC.UserInputService.ShowSingleInputDialog("Rename track", "Input a new name for this track", track.DisplayName) is string newDisplayName) {
                track.DisplayName = newDisplayName;
            }
        }
    }
}