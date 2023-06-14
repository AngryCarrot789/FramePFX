using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Layers;
using FramePFX.Core.Utils;

namespace FramePFX.Editor.Timeline.Actions {
    [ActionRegistration("actions.editor.NewVideoLayer")]
    public class NewVideoLayerAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            TimelineViewModel timeline = EditorActionUtils.FindTimeline(e.DataContext);
            if (timeline == null) {
                if (e.IsUserInitiated) {
                    await IoC.MessageDialogs.ShowMessageAsync("No timeline available", "Create a new project to add a new video layer");
                }

                return true;
            }

            string name = TextIncrement.GetNextText(timeline.Layers.OfType<VideoLayerViewModel>().Select(x => x.DisplayName), "Video Layer");
            VideoLayerViewModel layer = await timeline.AddVideoLayerAction();
            layer.DisplayName = name;
            return true;
        }
    }

    [ActionRegistration("actions.editor.NewAudioLayer")]
    public class NewAudioLayerAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            TimelineViewModel timeline = EditorActionUtils.FindTimeline(e.DataContext);
            if (timeline == null) {
                if (e.IsUserInitiated) {
                    await IoC.MessageDialogs.ShowMessageAsync("No timeline available", "Create a new project to add a new audio layer");
                }

                return true;
            }

            string name = TextIncrement.GetNextText(timeline.Layers.OfType<AudioLayerViewModel>().Select(x => x.DisplayName), "Audio Layer");
            AudioLayerViewModel layer = await timeline.AddAudioLayerAction();
            layer.DisplayName = name;
            return true;
        }
    }
}