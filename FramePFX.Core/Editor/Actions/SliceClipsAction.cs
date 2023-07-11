using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ViewModels.Timelines;

namespace FramePFX.Core.Editor.Actions {
    [ActionRegistration("actions.editor.timeline.SliceClips")]
    public class SliceClipsAction : AnAction {
        public SliceClipsAction() {

        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            TimelineViewModel timeline = EditorActionUtils.FindTimeline(e.DataContext);
            if (timeline == null) {
                if (e.IsUserInitiated) {
                    await IoC.MessageDialogs.ShowMessageAsync("No timeline available", "Create a new project to cut clips");
                }

                return false;
            }

            long frame = timeline.PlayHeadFrame;
            List<ClipViewModel> selected = timeline.Tracks.SelectMany(x => x.SelectedClips).ToList();
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

            return true;
        }

        public override bool CanExecute(AnActionEventArgs e) {
            return EditorActionUtils.FindTimeline(e.DataContext) != null;
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