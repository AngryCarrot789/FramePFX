using FramePFX.ResourceManaging.Items;

namespace FramePFX.ResourceManaging.ViewModels.Items {
    public class ResourceMediaViewModel : ResourceItemViewModel {
        private string filePath;
        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
        }

        public ResourceMedia Media { get; set; }

        public ResourceMediaViewModel(ResourceManagerViewModel manager) : base(manager) {

        }
    }
}