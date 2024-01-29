using FramePFX.Editors.ResourceManaging.Resources;

namespace FramePFX.Editors.ResourceManaging.Autoloading {
    /// <summary>
    /// An entry that represents an invalid image path that does not exist or couldn't represent an image
    /// </summary>
    public class InvalidImagePathEntry : InvalidResourceEntry {
        public new ResourceImage Resource => (ResourceImage) base.Resource;

        private string filePath;

        public string FilePath {
            get => this.filePath;
            set {
                if (this.filePath == value)
                    return;
                this.filePath = value;
                this.FilePathChanged?.Invoke(this);
            }
        }

        public event InvalidResourceEntryEventHandler FilePathChanged;

        public InvalidImagePathEntry(ResourceImage resource) : base(resource) {
            this.DisplayName = "Invalid image file path";
        }
    }
}