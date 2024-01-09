using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceCompositionViewModel : ResourceItemViewModel {
        public new ResourceComposition Model => (ResourceComposition) base.Model;

        public TimelineViewModel Timeline { get; }

        public ResourceCompositionViewModel(ResourceComposition model) : base(model) {
            this.Timeline = new CompositionTimelineViewModel(model.Timeline);
        }

        public override void SetManager(ResourceManagerViewModel manager) {
            base.SetManager(manager);
            this.Timeline.SetProject(manager?.Project);
        }
    }
}