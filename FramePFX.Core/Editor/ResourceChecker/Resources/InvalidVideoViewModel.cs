using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;

namespace FramePFX.Core.Editor.ResourceChecker.Resources {
    public class InvalidVideoViewModel : InvalidResourceViewModel {
        public new ResourceOldMediaViewModel Resource => (ResourceOldMediaViewModel) base.Resource;

        private string filePath;
        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
        }

        public InvalidVideoViewModel(ResourceOldMediaViewModel resource) : base(resource) {
            this.filePath = resource.FilePath;
        }
    }
}
