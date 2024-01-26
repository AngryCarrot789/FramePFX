using System;
using SkiaSharp;

namespace FramePFX.Editors.Media_OLD.Images {
    public class ImageMediaSourceTrack : VideoMediaSourceTrack {
        public ImageMediaSource MediaSource { get; }

        public ImageMediaSourceTrack(ImageMediaSource source) {
            this.MediaSource = source;
        }

        public override void Seek(TimeSpan span) {

        }

        public override void CopyTo(SKCanvas canvas) {
            if (this.MediaSource.image != null) {
                canvas.DrawImage(this.MediaSource.image, new SKPoint());
            }
        }
    }
}