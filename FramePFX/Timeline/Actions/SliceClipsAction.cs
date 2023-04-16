using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Timeline.ViewModels;
using FramePFX.Timeline.ViewModels.Clips;
using FramePFX.Timeline.ViewModels.Layer;

namespace FramePFX.Timeline.Actions {
    [ActionRegistration("actions.editor.timeline.SliceClips")]
    public class CutClipsAction : AnAction {
        public CutClipsAction() : base(() => "Slice clips", () => "Slices your selection (or all clips) where the play head is") {
        }

        public override Task<bool> ExecuteAsync(AnActionEventArgs e) {
            EditorTimeline timeline = EditorActionUtils.FindTimeline(e.DataContext);
            if (timeline == null) {
                return Task.FromResult(false);
            }

            List<TimelineVideoClip> selected = timeline.Handle.GetSelectedClips().ToList();
            if (selected.Count < 1) {
                CutAllOnPlayHead(timeline);
            }
            else {
                long frame = timeline.PlayHeadFrame;
                selected.RemoveAll(x => !x.IntersectsFrameAt(frame));
                if (selected.Count > 0) {
                    CutAllAtFrame(timeline, selected, frame);
                }
            }

            return Task.FromResult(true);
        }

        public static void CutAllOnPlayHead(EditorTimeline timeline) {

        }

        public static void CutAllAtFrame(EditorTimeline timeline, List<TimelineVideoClip> clips, long frame) {

        }
    }
}