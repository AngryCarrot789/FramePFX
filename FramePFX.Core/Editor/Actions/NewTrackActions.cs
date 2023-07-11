using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ViewModels.Timelines;
using FramePFX.Core.Editor.ViewModels.Timelines.Tracks;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Actions {
    public class NewVideoTrackAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            TimelineViewModel timeline = EditorActionUtils.FindTimeline(e.DataContext);
            if (timeline == null) {
                if (e.IsUserInitiated) {
                    await IoC.MessageDialogs.ShowMessageAsync("No timeline available", "Create a new project to add a new video track");
                }

                return true;
            }

            string name = TextIncrement.GetNextText(timeline.Tracks.OfType<VideoTrackViewModel>().Select(x => x.DisplayName), "Video Track");
            VideoTrackViewModel track = await timeline.AddVideoTrackAction();
            track.DisplayName = name;
            return true;
        }
    }

    public class NewAudioTrackAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            TimelineViewModel timeline = EditorActionUtils.FindTimeline(e.DataContext);
            if (timeline == null) {
                if (e.IsUserInitiated) {
                    await IoC.MessageDialogs.ShowMessageAsync("No timeline available", "Create a new project to add a new audio track");
                }

                return true;
            }

            string name = TextIncrement.GetNextText(timeline.Tracks.OfType<AudioTrackViewModel>().Select(x => x.DisplayName), "Audio Track");
            AudioTrackViewModel track = await timeline.AddAudioTrackAction();
            track.DisplayName = name;
            return true;
        }
    }
}