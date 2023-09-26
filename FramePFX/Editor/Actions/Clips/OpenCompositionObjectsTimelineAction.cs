using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;

namespace FramePFX.Editor.Actions.Clips {
    [ActionRegistration("actions.timeline.OpenCompositionObjectsTimeline")]
    public class OpenCompositionObjectsTimelineAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (e.DataContext.TryGetContext(out CompositionVideoClipViewModel clip)) {
                if (clip.TryGetResource(out ResourceCompositionViewModel resource)) {
                    await OpenTimeline(resource);
                }
            }
            else if (e.DataContext.TryGetContext(out ResourceCompositionViewModel resource)) {
                await OpenTimeline(resource);
            }
            else {
                return false;
            }

            return true;
        }

        public static async Task OpenTimeline(ResourceCompositionViewModel composition) {
            if (composition.Manager?.Project.Editor == null) {
                return;
            }

            composition.Manager.Project.Editor.OpenAndSelectTimeline(composition.Timeline);
            await composition.Timeline.DoAutomationTickAndRenderToPlayback();
        }
    }
}