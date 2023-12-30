using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions.Tracks {
    public class DeleteSelectedTracksAction : ContextAction {
        public override bool CanExecute(ContextActionEventArgs e) {
            return EditorActionUtils.GetTimeline(e.DataContext, out TimelineViewModel timeline);
        }

        public override async Task ExecuteAsync(ContextActionEventArgs e) {
            TimelineViewModel timeline = null;
            if (e.DataContext.TryGetContext(out TrackViewModel targetTrack) && (timeline = targetTrack.Timeline) != null) {
                if (!timeline.SelectedTracks.Contains(targetTrack)) {
                    timeline.RemoveTrack(targetTrack);
                    return;
                }
            }

            HashSet<TrackViewModel> tracks = new HashSet<TrackViewModel>();
            if (timeline == null && !EditorActionUtils.GetTimeline(e.DataContext, out timeline)) {
                return;
            }

            await timeline.RemoveTracksAction(tracks, true);
        }

        public static async Task CutAllOnPlayHead(TimelineViewModel timeline) {
            long frame = timeline.PlayHeadFrame;
            List<ClipViewModel> list = new List<ClipViewModel>();
            foreach (TrackViewModel track in timeline.Tracks) {
                list.AddRange(track.Clips);
            }

            if (list.Count > 0) {
                foreach (ClipViewModel clip in list) {
                    if (clip.IntersectsFrameAt(frame)) {
                        await clip.Track.SliceClipAction(clip, frame);
                    }
                }
            }
        }
    }
}