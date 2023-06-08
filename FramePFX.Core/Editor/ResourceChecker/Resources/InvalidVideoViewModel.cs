using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;

namespace FramePFX.Core.Editor.ResourceChecker.Resources {
    public class InvalidVideoViewModel : InvalidResourceViewModel {
        public new ResourceMediaViewModel Resource => (ResourceMediaViewModel) base.Resource;

        private string filePath;
        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
        }

        public InvalidVideoViewModel(ResourceMediaViewModel resource) : base(resource) {
            this.filePath = resource.FilePath;
        }
    }
}
