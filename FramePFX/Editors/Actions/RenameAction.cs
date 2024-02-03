using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public abstract class RenameAction : AnAction {
    }

    public class RenameResourceAction : AnAction {
        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(DataKeys.ResourceObjectKey, out BaseResource resource)) {
                return Task.CompletedTask;
            }

            if (IoC.UserInputService.ShowSingleInputDialog("Rename resource item", "Input a new name for this resource", resource.DisplayName) is string newDisplayName) {
                resource.DisplayName = newDisplayName;
            }

            return Task.CompletedTask;
        }
    }

    public class RenameClipAction : AnAction {
        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(DataKeys.ClipKey, out Clip clip)) {
                return Task.CompletedTask;
            }

            if (IoC.UserInputService.ShowSingleInputDialog("Rename clip", "Input a new name for this clip", clip.DisplayName) is string newDisplayName) {
                clip.DisplayName = newDisplayName;
            }

            return Task.CompletedTask;
        }
    }

    public class RenameTrackAction : AnAction {
        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(DataKeys.TrackKey, out Track track)) {
                return Task.CompletedTask;
            }

            if (IoC.UserInputService.ShowSingleInputDialog("Rename track", "Input a new name for this track", track.DisplayName) is string newDisplayName) {
                track.DisplayName = newDisplayName;
            }

            return Task.CompletedTask;
        }
    }
}