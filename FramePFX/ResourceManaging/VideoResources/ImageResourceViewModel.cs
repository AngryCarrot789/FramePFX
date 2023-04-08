using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FramePFX.ResourceManaging.VideoResources {
    public class ImageResourceViewModel : ResourceItemViewModel {
        private string filePath;
        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
        }

        public Image<Bgra32> ImageData { get; set; }

        public ImageResourceViewModel() {

        }
    }
}