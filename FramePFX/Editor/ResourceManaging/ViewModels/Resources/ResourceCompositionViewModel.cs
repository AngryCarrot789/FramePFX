using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceCompositionViewModel : ResourceItemViewModel {
        public new ResourceCompositionSeq Model => (ResourceCompositionSeq) base.Model;

        public TimelineViewModel Timeline { get; }

        public ResourceCompositionViewModel(ResourceCompositionSeq model) : base(model) {
            this.Timeline = new TimelineViewModel(model.Timeline);
        }

        public override void SetManager(ResourceManagerViewModel manager) {
            base.SetManager(manager);
            this.Timeline.Project = manager?.Project;
        }
    }
}