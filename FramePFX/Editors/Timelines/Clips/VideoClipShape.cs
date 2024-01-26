using System.Numerics;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Effects;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Clips {
    /// <summary>
    /// A video clip that draws a basic square, used as a debug video clip mostly
    /// </summary>
    public class VideoClipShape : VideoClip {
        private RenderData renderData;

        public Vector2 RectSize { get; set; } = new Vector2(100, 40);

        public Vector2 PointDemoHelper {
            get {
                MotionEffect fx = (MotionEffect) this.Effects[0];
                return new Vector2(fx.MediaPositionX, fx.MediaPositionY);
            }
            set {
                MotionEffect fx = (MotionEffect) this.Effects[0];
                fx.MediaPositionX = value.X;
                fx.MediaPositionY = value.Y;
            }
        }

        public VideoClipShape() {
            this.UsesCustomOpacityCalculation = true;

            // demo -- add a sample opacity automation key frame
            this.AutomationData[OpacityParameter].AddNewKeyFrame(0, out _);

            this.AddEffect(new MotionEffect());
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
            using (SKPaint paint = new SKPaint() {Color = colour}) {
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