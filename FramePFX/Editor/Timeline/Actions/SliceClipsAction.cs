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
            List<VideoClipViewModel> selected = timeline.Layers.SelectMany(x => x.SelectedClips).OfType<VideoClipViewModel>().ToList();
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
            TimelineViewModel timeline = EditorActionUtils.FindTimeline(e.DataContext);
            return timeline == null ? Presentation.VisibleAndDisabled : base.GetPresentation(e);
        }

        public static void CutAllOnPlayHead(TimelineViewModel timeline) {
            long frame = timeline.PlayHeadFrame;
            List<VideoClipViewModel> list = new List<VideoClipViewModel>();
            foreach (TimelineLayerViewModel layer in timeline.Layers) {
                foreach (VideoClipViewModel clip in layer.Clips) {
                    if (clip.IntersectsFrameAt(frame)) {
                        list.Add(clip);
                    }
                }
            }

            if (list.Count > 0) {
                CutAllAtFrame(timeline, list, frame);
            }
        }

        public static void CutAllAtFrame(TimelineViewModel timeline, IEnumerable<VideoClipViewModel> clips, long frame) {
            foreach (VideoClipViewModel clip in clips) {
                if (!clip.IntersectsFrameAt(frame)) { // shouldn't return false
                    continue;
                }

                if (!(clip.Layer is VideoLayerViewModel videoLayerModel)) {
                    continue;
                }

                videoLayerModel.SliceClip(clip, frame);
            }
        }
    }
}