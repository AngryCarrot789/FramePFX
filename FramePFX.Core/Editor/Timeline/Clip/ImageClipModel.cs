using System.Numerics;
using FramePFX.Core.Rendering;
using FramePFX.Core.ResourceManaging.Resources;
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

        public override void Render(RenderContext ctx, long frame) {
            if (!this.TryGetResource(out ResourceImage resource)) {
                return;
            }

            if (resource.image == null) {
                return;
            }

            this.Transform(ctx.Canvas, out _, out var matrix);
            ctx.Canvas.DrawImage(resource.image, 0, 0);
            ctx.Canvas.SetMatrix(matrix);
        }
    }
}