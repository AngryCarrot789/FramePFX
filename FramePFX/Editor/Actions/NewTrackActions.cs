using System.Linq;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Tracks;
using FramePFX.Utils;

namespace FramePFX.Editor.Actions {
    public class NewVideoTrackAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!EditorActionUtils.GetNewTrackData(e.DataContext, out TimelineViewModel timeline, out int index, AVType.Video)) {
                return true;
            }

            string newName = TextIncrement.GetNextText(timeline.Tracks.OfType<VideoTrackViewModel>().Select(x => x.DisplayName), "Video Track");
            VideoTrackViewModel track = await timeline.InsertNewVideoTrackAction(index);
            // timeline.SelectedTracks.ClearAndAdd(track);
            track.DisplayName = newName;
            return true;
        }
    }

    public class NewAudioTrackAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!EditorActionUtils.GetNewTrackData(e.DataContext, out TimelineViewModel timeline, out int index, AVType.Audio)) {
                return true;
            }

            string newName = TextIncrement.GetNextText(timeline.Tracks.OfType<AudioTrackViewModel>().Select(x => x.DisplayName), "Audio Track");
            AudioTrackViewModel track = await timeline.InsertNewAudioTrackAction(index);
            // timeline.SelectedTracks.ClearAndAdd(track);
            track.DisplayName = newName;
            return true;
        }
    }
}