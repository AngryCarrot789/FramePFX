using System.Numerics;
using FramePFX.Core.Rendering;
using FramePFX.Core.ResourceManaging.Resources;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.Clip {
    public class SquareClipModel : BaseResourceVideoClip<ResourceARGB> {

        public float Width { get; set; }

        public float Height { get; set; }

        public SquareClipModel() {

        }

        protected override void OnResourceStateChanged() {
            base.OnResourceStateChanged();
        }

        public override Vector2 GetSize() {
            return new Vector2(this.Width, this.Height);
        }

        public override void Render(RenderContext ctx, long frame) {
            if (!this.TryGetResource(out ResourceARGB c)) {
                return;
            }

            this.Transform(ctx.Canvas, out Rect rect, out SKMatrix oldMatrix);
            ctx.Canvas.DrawRect(rect.X1, rect.Y1, rect.Width, rect.Height, new SKPaint() {
                Color = new SKColor(c.ByteR, c.ByteG, c.ByteB, c.ByteA)
            });

            ctx.Canvas.SetMatrix(oldMatrix);
        }
    }
}