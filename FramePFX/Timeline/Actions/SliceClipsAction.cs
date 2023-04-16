using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using FramePFX.Core;
using FramePFX.Core.Actions;
using FramePFX.Timeline.ViewModels;
using FramePFX.Timeline.ViewModels.Clips;
using FramePFX.Timeline.ViewModels.Layer;

namespace FramePFX.Timeline.Actions {
    [ActionRegistration("actions.editor.timeline.SliceClips")]
    public class SliceClipsAction : AnAction {
        public SliceClipsAction() : base(() => "Slice clips", () => "Slices your selection (or all clips) where the play head is") {

        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            EditorTimeline timeline = EditorActionUtils.FindTimeline(e.DataContext);
            if (timeline == null) {
                if (e.IsUserInitiated) {
                    await CoreIoC.MessageDialogs.ShowMessageAsync("No timeline available", "Create a new project to cut clips");
                }

                return false;
            }

            List<TimelineVideoClip> selected = timeline.Handle.GetSelectedClips().ToList();
            if (selected.Count < 1) {
                CutAllOnPlayHead(timeline);
            }
            else {
                long frame = timeline.PlayHeadFrame;
                selected.RemoveAll(x => !x.IntersectsFrameAt(frame));
                if (selected.Count > 0) {
                    CutAllAtFrame(timeline, new List<BaseTimelineClip>(selected), frame);
                }
            }

            return true;
        }

        public override Presentation GetPresentation(AnActionEventArgs e) {
            EditorTimeline timeline = EditorActionUtils.FindTimeline(e.DataContext);
            return timeline == null ? Presentation.VisibleAndDisabled : base.GetPresentation(e);
        }

        public static void CutAllOnPlayHead(EditorTimeline timeline) {
            long frame = timeline.PlayHeadFrame;
            List<BaseTimelineClip> list = new List<BaseTimelineClip>();
            foreach (BaseTimelineLayer layer in timeline.Layers) {
                foreach (BaseTimelineClip clip in layer.Clips) {
                    if (clip.IntersectsFrameAt(frame)) {
                        list.Add(clip);
                    }
                }
            }

            if (list.Count > 0) {
                CutAllAtFrame(timeline, list, frame);
            }
        }

        public static void CutAllAtFrame(EditorTimeline timeline, List<BaseTimelineClip> clips, long frame) {
            foreach (BaseTimelineClip clip in clips) {
                if (!clip.IntersectsFrameAt(frame)) { // shouldn't return false
                    continue;
                }

                clip.Layer.SliceClip(clip, frame);
            }
        }
    }
}