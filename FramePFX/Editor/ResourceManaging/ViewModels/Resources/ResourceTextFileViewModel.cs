using FramePFX.Editor.ResourceManaging.Resources;

namespace FramePFX.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceTextFileViewModel : ResourceItemViewModel {
        public new ResourceTextFile Model => (ResourceTextFile) base.Model;

        public ResourceTextFileViewModel(ResourceTextFile model) : base(model) {
        }
    }
}