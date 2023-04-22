using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core;
using FramePFX.Core.Actions;
using FramePFX.Editor.Timeline.ViewModels;
using FramePFX.Editor.Timeline.ViewModels.Clips;
using FramePFX.Editor.Timeline.ViewModels.Layer;

namespace FramePFX.Editor.Timeline.Actions {
    [ActionRegistration("actions.editor.timeline.SliceClips")]
    public class SliceClipsAction : AnAction {
        public SliceClipsAction() : base(() => "Slice clips", () => "Slices your selection (or all clips) where the play head is") {

        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            PFXTimeline timeline = EditorActionUtils.FindTimeline(e.DataContext);
            if (timeline == null) {
                if (e.IsUserInitiated) {
                    await IoC.MessageDialogs.ShowMessageAsync("No timeline available", "Create a new project to cut clips");
                }

                return false;
            }

            long frame = timeline.PlayHeadFrame;
            List<PFXVideoClip> selected = timeline.Handle.GetSelectedClips().ToList();
            selected.RemoveAll(x => !x.IntersectsFrameAt(frame));
            if (selected.Count > 0) {
                CutAllAtFrame(timeline, selected, frame);
            }
            else {
                CutAllOnPlayHead(timeline);
            }

            return true;
        }

        public override Presentation GetPresentation(AnActionEventArgs e) {
            PFXTimeline timeline = EditorActionUtils.FindTimeline(e.DataContext);
            return timeline == null ? Presentation.VisibleAndDisabled : base.GetPresentation(e);
        }

        public static void CutAllOnPlayHead(PFXTimeline timeline) {
            long frame = timeline.PlayHeadFrame;
            List<PFXBaseClip> list = new List<PFXBaseClip>();
            foreach (PFXTimelineLayer layer in timeline.Layers) {
                foreach (PFXBaseClip clip in layer.Clips) {
                    if (clip.IntersectsFrameAt(frame)) {
                        list.Add(clip);
                    }
                }
            }

            if (list.Count > 0) {
                CutAllAtFrame(timeline, list, frame);
            }
        }

        public static void CutAllAtFrame(PFXTimeline timeline, IEnumerable<PFXBaseClip> clips, long frame) {
            foreach (PFXBaseClip clip in clips) {
                if (!clip.IntersectsFrameAt(frame)) { // shouldn't return false
                    continue;
                }

                clip.Layer.SliceClip(clip, frame);
            }
        }
    }
}