using FramePFX.Core.Editor.ResourceManaging.Resources;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceMediaViewModel : ResourceItemViewModel {
        public new ResourceMedia Model => (ResourceMedia) base.Model;

        public string FilePath {
            get => this.Model.FilePath;
            set {
                this.Model.FilePath = value;
                this.RaisePropertyChanged();
            }
        }

        public ResourceMediaViewModel(ResourceManagerViewModel manager, ResourceMedia media) : base(manager, media) {

        }
    }
}