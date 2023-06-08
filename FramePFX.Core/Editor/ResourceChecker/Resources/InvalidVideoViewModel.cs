using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;

namespace FramePFX.Core.Editor.ResourceChecker.Resources {
    public class InvalidImageViewModel : InvalidResourceViewModel {
        public new ResourceImageViewModel Resource => (ResourceImageViewModel) base.Resource;

        private string filePath;
        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
        }

        public InvalidImageViewModel(ResourceImageViewModel resource) : base(resource) {
            this.filePath = resource.FilePath;
        }
    }
}
