using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions.Clips {
    public class SliceClipsAction : ContextAction {
        public SliceClipsAction() : base() {
        }

        public override bool CanExecute(ContextActionEventArgs e) {
            return EditorActionUtils.GetTimeline(e.DataContext, out TimelineViewModel timeline);
        }

        public override async Task ExecuteAsync(ContextActionEventArgs e) {
            if (!EditorActionUtils.GetTimeline(e.DataContext, out TimelineViewModel timeline)) {
                if (e.IsUserInitiated) {
                    await IoC.DialogService.ShowMessageAsync("No timeline available", "Create a new project to cut clips");
                }

                return;
            }

            long frame = timeline.PlayHeadFrame;
            List<ClipViewModel> selected = timeline.GetSelectedClips().ToList();
            if (selected.Count > 0) {
                foreach (ClipViewModel clip in (IEnumerable<ClipViewModel>) selected) {
                    if (clip.IntersectsFrameAt(frame)) {
                        await clip.Track.SliceClipAction(clip, frame);
                    }
                }
            }
            else {
                await CutAllOnPlayHead(timeline);
            }
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