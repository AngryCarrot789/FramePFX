using System.Numerics;
using FramePFX.Editors.Rendering;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Clips {
    /// <summary>
    /// A video clip that draws a basic square, used as a debug video clip mostly
    /// </summary>
    public class VideoClipShape : VideoClip {
        private RenderData renderData;

        public Vector2 RectSize { get; set; } = new Vector2(100, 40);

        public VideoClipShape() {
            this.UsesCustomOpacityCalculation = true;
        }

        public override Vector2? GetRenderSize() {
            return new Vector2(this.RectSize.X, this.RectSize.Y);
        }

        public override bool PrepareRenderFrame(PreRenderContext ctx, long frame) {
            this.renderData = new RenderData() {
                opacity = this.Opacity,
                size = this.RectSize,
                colour = this.Track?.Colour ?? SKColors.White
            };

            return true;
        }

        public override void RenderFrame(RenderContext rc) {
            RenderData d = this.renderData;
            SKColor colour = RenderUtils.BlendAlpha(d.colour, d.opacity);
            using (SKPaint paint = new SKPaint() {Color = colour, IsAntialias = true}) {
                rc.Canvas.DrawRect(0, 0, d.size.X, d.size.Y, paint);
            }
        }

        private struct RenderData {
            public double opacity; // the clip opacity
            public Vector2 size;
            public SKColor colour;
        }
    }
}