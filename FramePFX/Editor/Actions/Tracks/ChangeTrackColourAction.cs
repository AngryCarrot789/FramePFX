using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels.Timelines;
using Gpic.Core.Services;
using SkiaSharp;

namespace FramePFX.Editor.Actions.Tracks {
    [ActionRegistration("actions.timeline.track.ChangeTrackColour")]
    public class ChangeTrackColourAction : ContextAction {
        public override bool CanExecute(ContextActionEventArgs e) {
            return EditorActionUtils.GetTrack(e.DataContext, out TrackViewModel track);
        }

        public override async Task ExecuteAsync(ContextActionEventArgs e) {
            if (EditorActionUtils.GetTrack(e.DataContext, out TrackViewModel track)) {
                IColourPicker picker = IoC.GetService<IColourPicker>();
                if (picker.PickARGB(track.TrackColour) is SKColor colour) {
                    track.TrackColour = colour;
                }
            }
        }
    }
}