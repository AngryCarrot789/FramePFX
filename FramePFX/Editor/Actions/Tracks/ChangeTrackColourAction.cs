using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels.Timelines;
using Gpic.Core.Services;
using SkiaSharp;

namespace FramePFX.Editor.Actions.Tracks {
    [ActionRegistration("actions.timeline.track.ChangeTrackColour")]
    public class ChangeTrackColourAction : AnAction {
        public override bool CanExecute(AnActionEventArgs e) {
            return EditorActionUtils.GetTrack(e.DataContext, out TrackViewModel track);
        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!EditorActionUtils.GetTrack(e.DataContext, out TrackViewModel track)) {
                return false;
            }

            IColourPicker picker = Services.GetService<IColourPicker>();
            if (picker.PickARGB(track.TrackColour) is SKColor colour) {
                track.TrackColour = colour;
            }

            return true;
        }
    }
}