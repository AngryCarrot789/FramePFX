using System;
using System.Numerics;
using FramePFX.Rendering;
using SkiaSharp;

namespace FramePFX.Editor.Timelines.Effects.Video
{
    public class TwirlEffect : VideoEffect
    {
        private SKRuntimeEffect effect;
        private SKShader shader;

        public TwirlEffect()
        {
        }

        protected override void OnRemovedFromClip()
        {
            base.OnRemovedFromClip();
            this.shader.Dispose();
            this.shader = null;
            this.effect.Dispose();
            this.effect = null;
        }

        public override void PostProcessFrame(long frame, RenderContext rc, Vector2? frameSize)
        {
            Vector2 size = frameSize ?? rc.FrameSize;
            SKPoint center = new SKPoint(size.X / 2f, size.Y / 2f);
        }
    }
}