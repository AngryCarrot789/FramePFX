using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FramePFX.ResourceManaging.Items {
    public class ResourceImage : ResourceItem {
        private string filePath;
        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
        }

        /// <summary>
        /// Whether this image resource is file based or not. Non-file based images were created in the editor
        /// and will be saved in the project folder eventually
        /// </summary>
        public bool IsFileBased => !string.IsNullOrEmpty(this.FilePath);

        /// <summary>
        /// This image's cached data. Will be set/updated when not set, manually refreshes, or the window is "activated"
        /// </summary>
        public Image<Bgra32> ImageData { get; set; }
    }
}