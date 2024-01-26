using System.IO;
using SkiaSharp;

namespace FramePFX.Editors.Media_OLD.Images {
    public class ImageMediaSource : MediaSource {
        public string FilePath { get; private set; }

        public SKImage image;
        public SKBitmap bitmap;

        public ImageMediaSource() {
            this.AddTrack(new ImageMediaSourceTrack(this));
        }

        /// <summary>
        /// Sets the file path that this media source will read from
        /// </summary>
        /// <param name="path"></param>
        public void SetImagePath(string path) {
            if (this.FilePath == path) {
                return;
            }

            this.FilePath = path;
            SKBitmap bmp = null;
            SKImage img = null;
            try {
                using (BufferedStream stream = new BufferedStream(File.OpenRead(path), 32768)) {
                    bmp = SKBitmap.Decode(stream);
                    img = SKImage.FromBitmap(bmp);
                }
            }
            catch {
                bmp?.Dispose();
                img?.Dispose();
                throw;
            }

            if (this.bitmap != null || this.image != null) {
                this.bitmap?.Dispose();
                this.bitmap = null;
                this.image?.Dispose();
                this.image = null;
            }

            this.image = img;
            this.bitmap = bmp;
        }
    }
}