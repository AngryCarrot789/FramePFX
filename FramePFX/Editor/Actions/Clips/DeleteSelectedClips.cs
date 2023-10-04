using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions.Clips
{
    public class DeleteSelectedClips : AnAction
    {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e)
        {
            if (!EditorActionUtils.GetTimeline(e.DataContext, out TimelineViewModel timeline))
            {
                if (e.IsUserInitiated)
                {
                    await Services.DialogService.ShowMessageAsync("No timeline available", "Create a new project to cut clips");
                }

                return false;
            }

            foreach (TrackViewModel track in timeline.Tracks.ToList())
            {
                if (track.SelectedClips.Count > 0)
                {
                    await track.DisposeAndRemoveItemsAction(track.SelectedClips);
                }
            }

            return true;
        }

        public override bool CanExecute(AnActionEventArgs e)
        {
            return EditorActionUtils.GetTimeline(e.DataContext, out TimelineViewModel timeline);
        }

        public static async Task CutAllOnPlayHead(TimelineViewModel timeline)
        {
            long frame = timeline.PlayHeadFrame;
            List<ClipViewModel> list = new List<ClipViewModel>();
            foreach (TrackViewModel track in timeline.Tracks)
            {
                list.AddRange(track.Clips);
            }

            if (list.Count > 0)
            {
                foreach (ClipViewModel clip in list)
                {
                    if (clip.IntersectsFrameAt(frame))
                    {
                        await clip.Track.SliceClipAction(clip, frame);
                    }
                }
            }
        }
    }
}