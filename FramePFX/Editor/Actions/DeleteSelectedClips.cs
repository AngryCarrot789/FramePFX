using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.History;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.History.ViewModels;
using FramePFX.RBC;

namespace FramePFX.Editor.Actions {
    public class DeleteSelectedClips : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            TimelineViewModel timeline = EditorActionUtils.FindTimeline(e.DataContext);
            if (timeline == null) {
                if (e.IsUserInitiated) {
                    await Services.DialogService.ShowMessageAsync("No timeline available", "Create a new project to cut clips");
                }

                return false;
            }

            bool deleted = false;
            List<List<RBEDictionary>> list = new List<List<RBEDictionary>>();
            foreach (TrackViewModel track in timeline.Tracks.ToList()) {
                List<RBEDictionary> clips = new List<RBEDictionary>();
                if (track.SelectedClips.Count > 0) {
                    List<ClipViewModel> selection = track.SelectedClips.ToList();
                    clips.AddRange(selection.Select(clip => Clip.WriteSerialisedWithId(clip.Model)));
                    await track.DisposeAndRemoveItemsAction(selection);
                    deleted = true;
                }

                list.Add(clips);
            }

            if (deleted) {
                HistoryManagerViewModel.Instance.AddAction(new HistoryClipDeletion(timeline, list));
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