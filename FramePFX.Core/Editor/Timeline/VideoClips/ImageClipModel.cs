using System.Numerics;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Rendering;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.VideoClips {
    public class ImageClipModel : BaseResourceClip<ResourceImage> {
        public ImageClipModel() {

        }

        protected override void OnResourceDataModified(string property) {
            switch (property) {
                case nameof(ResourceImage.FilePath):
                case nameof(ResourceImage.IsRawBitmapMode):
                    this.InvalidateRender();
                    break;
            }
        }

        public override Vector2? GetSize() {
            if (this.ResourcePath == null || !this.ResourcePath.TryGetResource(out ResourceImage r)) {
                return Vector2.Zero;
            }

            SKImage img = r.image;
            return img == null ? Vector2.Zero : new Vector2(img.Width, img.Height);
        }

        public override void Render(RenderContext render, long frame) {
            if (!this.TryGetResource(out ResourceImage resource))
                return;
            if (resource.image == null)
                return;

            this.Transform(render);
            render.Canvas.DrawImage(resource.image, 0, 0);
        }

        protected override ClipModel NewInstance() {
            return new ImageClipModel();
        }
    }
}