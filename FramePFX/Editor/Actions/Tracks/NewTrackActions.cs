using System.Linq;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Tracks;
using FramePFX.Utils;

namespace FramePFX.Editor.Actions.Tracks {
    public class NewVideoTrackAction : ContextAction {
        public override bool CanExecute(ContextActionEventArgs e) {
            return EditorActionUtils.GetTimeline(e.DataContext, out TimelineViewModel timeline);
        }

        public override async Task ExecuteAsync(ContextActionEventArgs e) {
            if (EditorActionUtils.GetNewTrackData(e.DataContext, out TimelineViewModel timeline, out int index, AVType.Video)) {
                string newName = TextIncrement.GetNextText(timeline.Tracks.OfType<VideoTrackViewModel>().Select(x => x.DisplayName), "Video Track");
                VideoTrackViewModel track = await timeline.InsertNewVideoTrackAction(index);
                // timeline.SelectedTracks.ClearAndAdd(track);
                track.DisplayName = newName;
            }
        }
    }

    public class NewAudioTrackAction : ContextAction {
        public override bool CanExecute(ContextActionEventArgs e) {
            return EditorActionUtils.GetTimeline(e.DataContext, out TimelineViewModel timeline);
        }

        public override async Task ExecuteAsync(ContextActionEventArgs e) {
            if (EditorActionUtils.GetNewTrackData(e.DataContext, out TimelineViewModel timeline, out int index, AVType.Audio)) {
                string newName = TextIncrement.GetNextText(timeline.Tracks.OfType<AudioTrackViewModel>().Select(x => x.DisplayName), "Audio Track");
                AudioTrackViewModel track = await timeline.InsertNewAudioTrackAction(index);
                // timeline.SelectedTracks.ClearAndAdd(track);
                track.DisplayName = newName;
            }
        }
    }
}