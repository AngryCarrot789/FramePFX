using System.Numerics;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.Clip {
    public class SquareClipModel : BaseResourceVideoClip<ResourceARGB> {

        public float Width { get; set; }

        public float Height { get; set; }

        public SquareClipModel() {

        }

        protected override void OnResourceOnlineChanged() {
            base.OnResourceOnlineChanged();
        }

        public override Vector2 GetSize() {
            return new Vector2(this.Width, this.Height);
        }

        public override void Render(RenderContext render, long frame) {
            if (!this.TryGetResource(out ResourceARGB c)) {
                return;
            }

            this.Transform(render.Canvas, out Rect rect, out SKMatrix oldMatrix);
            render.Canvas.DrawRect(rect.X1, rect.Y1, rect.Width, rect.Height, new SKPaint() {
                Color = new SKColor(c.ByteR, c.ByteG, c.ByteB, c.ByteA)
            });

            render.Canvas.SetMatrix(oldMatrix);
        }

        protected override void OnResourceModified(ResourceARGB resource, string property) {
            base.OnResourceModified(resource, property);
            switch (property) {
                case nameof(resource.A):
                case nameof(resource.R):
                case nameof(resource.G):
                case nameof(resource.B):
                    this.OnRenderInvalidated();
                    break;
            }
        }
    }
}