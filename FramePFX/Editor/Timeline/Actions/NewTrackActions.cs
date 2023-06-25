using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Tracks;
using FramePFX.Core.Utils;

namespace FramePFX.Editor.Timeline.Actions {
    [ActionRegistration("actions.editor.NewVideoTrack")]
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

    [ActionRegistration("actions.editor.NewAudioTrack")]
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