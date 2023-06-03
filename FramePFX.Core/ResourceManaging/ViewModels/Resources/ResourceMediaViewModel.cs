using FramePFX.Core.ResourceManaging.Resources;

namespace FramePFX.Core.ResourceManaging.ViewModels.Resources {
    public class ResourceMediaViewModel : ResourceItemViewModel {
        public new ResourceMedia Model => (ResourceMedia) base.Model;

        public string FilePath {
            get => this.Model.FilePath;
            set {
                this.Model.FilePath = value;
                this.RaisePropertyChanged();
                this.Model.OnModified?.Invoke(this.Model, nameof(this.Model.FilePath));
            }
        }

        public ResourceMediaViewModel(ResourceManagerViewModel manager, ResourceMedia media) : base(manager, media) {

        }
    }
}