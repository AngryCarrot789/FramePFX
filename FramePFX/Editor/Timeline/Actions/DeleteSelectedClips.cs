using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.History.ViewModels;
using FramePFX.Core.RBC;

namespace FramePFX.Editor.Timeline.Actions {
    [ActionRegistration("actions.editor.timeline.DeleteSelectedClips")]
    public class DeleteSelectedClips : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            TimelineViewModel timeline = EditorActionUtils.FindTimeline(e.DataContext);
            if (timeline == null) {
                if (e.IsUserInitiated) {
                    await IoC.MessageDialogs.ShowMessageAsync("No timeline available", "Create a new project to cut clips");
                }

                return false;
            }

            bool deleted = false;
            List<List<RBEDictionary>> historyList = new List<List<RBEDictionary>>();
            HistoryManagerViewModel history = timeline.Project.Editor.HistoryManager;
            foreach (LayerViewModel layer in timeline.Layers.ToList()) {
                List<RBEDictionary> clips = new List<RBEDictionary>();
                if (layer.SelectedClips.Count > 0) {
                    List<ClipViewModel> selection = layer.SelectedClips.ToList();
                    foreach (ClipViewModel clip in selection) {
                        RBEDictionary dictionary = new RBEDictionary();
                        clip.Model.WriteToRBE(dictionary);
                        clips.Add(dictionary);
                    }

                    await layer.DisposeAndRemoveItemsAction(selection);
                    deleted = true;
                }

                historyList.Add(clips);
            }

            if (deleted) {
                history.AddAction(new HistoryClipDeletion(timeline, historyList));
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
            foreach (LayerViewModel layer in timeline.Layers) {
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
    }
}