using System;
using System.Numerics;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Utils;
using FramePFX.Natives;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Effects {
    public class CPUPixelateEffect : VideoEffect {
        public static readonly ParameterLong BlockSizeParameter =
            Parameter.RegisterLong(
                typeof(CPUPixelateEffect),
                nameof(BlockSize),
                nameof(BlockSize),
                16, 0, 1024, // def/min/max
                ValueAccessors.LinqExpression<long>(typeof(CPUPixelateEffect), nameof(BlockSize)),
                ParameterFlags.StandardProjectVisual);

        public long BlockSize;

        private Vector2 renderSize;

        public override void PrepareRender(PreRenderContext ctx, long frame) {
            base.PrepareRender(ctx, frame);
            if (this.Owner is VideoClip) {
                this.renderSize = this.OwnerClip.GetRenderSize() ?? new Vector2();
            }
            else {
                this.renderSize = ctx.FrameSize;
            }
        }

        public override void PostProcessFrame(RenderContext rc, ref SKRect renderArea) {
            base.PostProcessFrame(rc, ref renderArea);

            // It should never be negative as it's guarded by the parameter system.... buuuuut just in case ;)
            if (this.BlockSize <= 0) {
                return;
            }

            unsafe {
                uint* pImg = (uint*) rc.Bitmap.GetPixels().ToPointer();
                SKRectI visibleI = renderArea.CeilAndFloorI();
                int left = Math.Max(0, visibleI.Left);
                int top = Math.Max(0, visibleI.Top);
                int right = Math.Min(rc.ImageInfo.Width, visibleI.Right);
                int bottom = Math.Min(rc.ImageInfo.Height, visibleI.Bottom);

                PFXNative.PFXCE_PixelateVfx(pImg, rc.ImageInfo.Width, rc.ImageInfo.Height, left, top, right, bottom, (int) this.BlockSize);
            }
        }
    }
}