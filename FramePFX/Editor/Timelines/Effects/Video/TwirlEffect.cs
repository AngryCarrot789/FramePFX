using System;
using System.Numerics;
using FramePFX.Rendering;
using SkiaSharp;

namespace FramePFX.Editor.Timelines.Effects.Video {
    public class TwirlEffect : VideoEffect {
        public TwirlEffect() {
        }

        public override void PostProcessFrame(long frame, RenderContext rc, Vector2? frameSize) {
            Vector2 size = frameSize ?? rc.FrameSize;
            SKPoint center = new SKPoint(size.X / 2f, size.Y / 2f);
            float radius = Math.Min(size.X, size.Y) / 2f;
            using (SKShader twirlMask = SKShader.CreateRadialGradient(center, radius, new SKColor[] {SKColors.Transparent, SKColors.Black}, null, SKShaderTileMode.Clamp)) {
                using (SKPaint maskPaint = new SKPaint {Shader = twirlMask}) {
                    rc.Canvas.DrawRect(new SKRect(0, 0, size.X, size.Y), maskPaint);
                }
            }
        }
    }
}