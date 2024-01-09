using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Utils;

namespace FramePFX.Editor.Actions.Clips {
    public class DuplicateClipsAction : ContextAction {
        public DuplicateClipsAction() : base() {
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

            List<ClipViewModel> selected = timeline.GetSelectedClips().ToList();
            if (selected.Count < 1) {
                return;
            }

            ClipViewModel lastClip = null;
            foreach (ClipViewModel clip in (IEnumerable<ClipViewModel>) selected) {
                FrameSpan span = Track.GetSpanUntilClipOrLimitedDuration(clip.Model.Track, clip.FrameEndIndex, clip.FrameDuration);
                lastClip = clip.Track.DuplicateClipAction(clip, span);
            }

            if (lastClip != null) {
                lastClip.Track.SelectedClip = lastClip;
            }
        }
    }
}