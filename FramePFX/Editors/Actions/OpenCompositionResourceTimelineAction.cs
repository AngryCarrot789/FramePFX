using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editors.Contextual;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Resources;

namespace FramePFX.Editors.Actions {
    public class OpenCompositionResourceTimelineAction : AnAction {
        public override bool CanExecute(AnActionEventArgs e) {
            if (!ResourceContextRegistry.GetSingleSelection(e.DataContext, out BaseResource resource))
                return false;
            return resource is ResourceComposition composition && composition.Manager.Project.ActiveTimeline != composition.Timeline;
        }

        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (ResourceContextRegistry.GetSingleSelection(e.DataContext, out BaseResource resource) && resource is ResourceComposition composition) {
                composition.Manager.Project.ActiveTimeline = composition.Timeline;
            }

            return Task.CompletedTask;
        }
    }
}