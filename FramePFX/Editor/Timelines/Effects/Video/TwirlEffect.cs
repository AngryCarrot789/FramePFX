using System;
using System.Numerics;
using FramePFX.Rendering;
using SkiaSharp;

namespace FramePFX.Editor.Timelines.Effects.Video {
    public class TwirlEffect : VideoEffect {
        private SKRuntimeEffect effect;
        private SKShader shader;

        public TwirlEffect() {
        }

        protected override void OnAddedToClip() {
            base.OnAddedToClip();
            // this.effect = SKRuntimeEffect.Create(ResourceLocator.ReadFile("ok.sksl") , out string err);
            // if (!string.IsNullOrWhiteSpace(err))
            //     throw new Exception(err);
            // this.shader = this.effect.ToShader(true, new SKRuntimeEffectUniforms(this.effect) {
            // });
        }

        protected override void OnRemovedFromClip() {
            base.OnRemovedFromClip();
            // this.shader.Dispose();
            // this.shader = null;
            // this.effect.Dispose();
            // this.effect = null;
        }

        public override void PostProcessFrame(long frame, RenderContext rc, Vector2? frameSize) {
            Vector2 size = frameSize ?? rc.FrameSize;
            // using (SKPaint paint = new SKPaint() {Shader = this.shader}) {
            //     rc.Canvas.DrawRect(new SKRect(0, 0, size.X, size.Y), paint);
            // }
            SKPoint center = new SKPoint(size.X / 2f, size.Y / 2f);
            float radius = Math.Min(size.X, size.Y) / 2f;
            using (SKShader twirlMask = SKShader.CreateRadialGradient(center, radius, new SKColor[] {SKColors.Transparent, SKColors.Black}, null, SKShaderTileMode.Clamp)) {
                using (SKPaint maskPaint = new SKPaint {Shader = twirlMask, }) {
                    rc.Canvas.DrawRect(new SKRect(0, 0, size.X, size.Y), maskPaint);
                }
            }
        }
    }
}