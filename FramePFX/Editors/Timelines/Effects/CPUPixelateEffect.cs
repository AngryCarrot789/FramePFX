using System;
using System.Numerics;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.DataTransfer;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Utils;
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
            ParameterFlags.AffectsRender);

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

        // Function to pixelate the image
        private static unsafe void pixelate(RenderContext rc, int srcWidth, int srcHeight, SKRect rect, int blockSize) {
            int* pImg = (int*) rc.Bitmap.GetPixels().ToPointer();
            SKRectI visibleI = rect.CeilAndFloorI();
            int left = Math.Max(0, visibleI.Left);
            int top = Math.Max(0, visibleI.Top);
            int right = Math.Min(srcWidth, visibleI.Right);
            int bottom = Math.Min(srcHeight, visibleI.Bottom);

            for (int blockY = top; blockY < bottom; blockY += blockSize) {
                for (int blockX = left; blockX < right; blockX += blockSize) {
                    uint totalR = 0, totalG = 0, totalB = 0, totalA = 0;
                    int pxTotal = 0;
                    for (int pY = blockY; pY < blockY + blockSize && pY < bottom; ++pY) {
                        for (int pX = blockX; pX < blockX + blockSize && pX < right; ++pX) {
                            uint pixel = (uint) pImg[pY * srcWidth + pX];
                            totalB += (pixel >> 0) & 255;
                            totalG += (pixel >> 8) & 255;
                            totalR += (pixel >> 16) & 255;
                            totalA += (pixel >> 24) & 255;
                            pxTotal++;
                        }
                    }

                    byte avgR = (byte) (totalR / pxTotal);
                    byte avgG = (byte) (totalG / pxTotal);
                    byte avgB = (byte) (totalB / pxTotal);
                    byte avgA = (byte) (totalA / pxTotal);
                    int pixelBgra = avgB | (avgG << 8) | (avgR << 16) | (avgA << 24);
                    for (int pY = blockY; pY < blockY + blockSize && pY < bottom; ++pY) {
                        for (int pX = blockX; pX < blockX + blockSize && pX < right; ++pX) {
                            pImg[pY * srcWidth + pX] = pixelBgra;
                        }
                    }
                }
            }
        }

        public override void PostProcessFrame(RenderContext rc, ref SKRect renderArea) {
            base.PostProcessFrame(rc, ref renderArea);

            // It should never be negative as it's guarded by the parameter system.... buuuuut just in case ;)
            if (this.BlockSize <= 0) {
                return;
            }

            pixelate(rc, rc.ImageInfo.Width, rc.ImageInfo.Height, renderArea, (int) this.BlockSize);
        }
    }
}