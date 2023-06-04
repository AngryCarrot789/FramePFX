using System.Numerics;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Rendering;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.Clip {
    public class ImageClipModel : BaseResourceVideoClip<ResourceImage> {
        public ImageClipModel() {

        }

        public override Vector2 GetSize() {
            if (!this.TryGetResource(out ResourceImage resource)) {
                return Vector2.Zero;
            }

            SKImage img = resource.image;
            return img == null ? Vector2.Zero : new Vector2(img.Width, img.Height);
        }

        public override void Render(RenderContext render, long frame) {
            if (!this.TryGetResource(out ResourceImage resource)) {
                return;
            }

            if (resource.image == null) {
                return;
            }

            this.Transform(render.Canvas, out _, out var matrix);
            render.Canvas.DrawImage(resource.image, 0, 0);
            render.Canvas.SetMatrix(matrix);
        }
    }
}