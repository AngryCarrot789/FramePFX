using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions.Clips {
    public class DeleteSelectedClips : AnAction {
        public DeleteSelectedClips() : base() {
        }

        public override bool CanExecute(AnActionEventArgs e) {
            return EditorActionUtils.GetTimeline(e.DataContext, out TimelineViewModel timeline);
        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            TimelineViewModel timeline = null;
            if (e.DataContext.TryGetContext(out ClipViewModel targetClip) && targetClip.Track != null) {
                if (!targetClip.Track.SelectedClips.Contains(targetClip)) {
                    targetClip.Track?.DisposeAndRemoveItemsAction(new List<ClipViewModel>() {targetClip});
                    return true;
                }

                timeline = targetClip.Track?.Timeline;
            }

            if (timeline != null || EditorActionUtils.GetTimeline(e.DataContext, out timeline)) {
                foreach (TrackViewModel track in timeline.Tracks.ToList()) {
                    if (track.SelectedClips.Count > 0) {
                        await track.DisposeAndRemoveItemsAction(track.SelectedClips);
                    }
                }
            }
            else {
                return false;
            }

            return true;
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