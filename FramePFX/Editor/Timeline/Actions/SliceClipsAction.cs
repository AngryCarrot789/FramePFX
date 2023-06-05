using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Core.Editor.ViewModels.Timeline.Layers;

namespace FramePFX.Editor.Timeline.Actions {
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
            List<ClipViewModel> selected = timeline.Layers.SelectMany(x => x.SelectedClips).ToList();
            if (selected.Count > 0) {
                foreach (ClipViewModel clip in (IEnumerable<ClipViewModel>) selected) {
                    if (clip.IntersectsFrameAt(frame)) {
                        await clip.Layer.SliceClipAction(clip, frame);
                    }
                }
            }
            else {
                await CutAllOnPlayHead(timeline);
            }

            return true;
        }

        public override Presentation GetPresentation(AnActionEventArgs e) {
            TimelineViewModel timeline = EditorActionUtils.FindTimeline(e.DataContext);
            return timeline == null ? Presentation.VisibleAndDisabled : base.GetPresentation(e);
        }

        public static async Task CutAllOnPlayHead(TimelineViewModel timeline) {
            long frame = timeline.PlayHeadFrame;
            List<ClipViewModel> list = new List<ClipViewModel>();
            foreach (TimelineLayerViewModel layer in timeline.Layers) {
                list.AddRange(layer.Clips);
            }

            if (list.Count > 0) {
                foreach (ClipViewModel clip in list) {
                    if (clip.IntersectsFrameAt(frame)) {
                        await clip.Layer.SliceClipAction(clip, frame);
                    }
                }
            }
        }

        // Unsafe as in assuming the given frame intersects all of the clips
    }
}