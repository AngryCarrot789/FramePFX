using FramePFX.CommandSystem;
using FramePFX.Editors.Contextual;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Resources;

namespace FramePFX.Editors.Actions {
    public class OpenCompositionResourceTimelineCommand : Command {
        public override bool CanExecute(CommandEventArgs e) {
            if (!ResourceContextRegistry.GetSingleSelection(e.DataContext, out BaseResource resource))
                return false;
            return resource is ResourceComposition composition && composition.Manager.Project.ActiveTimeline != composition.Timeline;
        }

        public override void Execute(CommandEventArgs e) {
            if (ResourceContextRegistry.GetSingleSelection(e.DataContext, out BaseResource resource) && resource is ResourceComposition composition) {
                composition.Manager.Project.ActiveTimeline = composition.Timeline;
            }
        }
    }
}