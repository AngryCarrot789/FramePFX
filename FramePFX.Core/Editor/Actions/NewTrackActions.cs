using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ViewModels.Timelines;
using FramePFX.Core.Editor.ViewModels.Timelines.Tracks;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Actions
{
    public class NewVideoTrackAction : AnAction
    {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e)
        {
            TimelineViewModel timeline = EditorActionUtils.FindTimeline(e.DataContext);
            if (timeline == null)
            {
                return false;
            }

            int index; // slightly compact code... get index of track otherwise get timeline track count
            if (!e.DataContext.TryGetContext(out TrackViewModel track) || (index = timeline.Tracks.IndexOf(track)) == -1)
                index = timeline.Tracks.Count;
            string newName = TextIncrement.GetNextText(timeline.Tracks.OfType<VideoTrackViewModel>().Select(x => x.DisplayName), "Video Track");
            track = await timeline.InsertNewVideoTrackAction(index);
            track.DisplayName = newName;
            return true;
        }
    }

    public class NewAudioTrackAction : AnAction
    {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e)
        {
            TimelineViewModel timeline = EditorActionUtils.FindTimeline(e.DataContext);
            if (timeline == null)
            {
                return false;
            }


            int index;
            if (!e.DataContext.TryGetContext(out TrackViewModel track) || (index = timeline.Tracks.IndexOf(track)) == -1)
                index = timeline.Tracks.Count;
            string newName = TextIncrement.GetNextText(timeline.Tracks.OfType<AudioTrackViewModel>().Select(x => x.DisplayName), "Audio Track");
            track = await timeline.InsertNewAudioTrackAction(index);
            track.DisplayName = newName;
            return true;
        }
    }
}