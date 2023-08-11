using FramePFX.Core.Editor.ResourceManaging.Resources;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceTextFileViewModel : ResourceItemViewModel {
        public new ResourceTextFile Model => (ResourceTextFile) base.Model;

        public ResourceTextFileViewModel(ResourceTextFile model) : base(model) {
        }
    }
}