namespace FramePFX.Core.ResourceManaging {
    public class ResourceImageViewModel : ResourceItemViewModel {
        private string filePath;
        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value, this.OnFilePathChanged);
        }

        public INativeImageResource Resource { get; set; }

        public ResourceImageViewModel() {

        }

        private void OnFilePathChanged() {
            this.Resource?.OnImageChanged(this.FilePath);
        }
    }
}