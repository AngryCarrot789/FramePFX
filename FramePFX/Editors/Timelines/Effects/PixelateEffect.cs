using System.Numerics;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.DataTransfer;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Clips;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Effects {
    public class PixelateEffect : VideoEffect {
        public static readonly DataParameterDouble BlockSizeParameter =
            DataParameter.Register(
                new DataParameterDouble(
                    typeof(PixelateEffect),
                    nameof(BlockSize), default(double),
                    ValueAccessors.Reflective<double>(typeof(PixelateEffect), nameof(BlockSize)),
                    DataParameterFlags.AffectsRender));

        private double blockSize;

        public double BlockSize {
            get => this.blockSize;
            set => DataParameter.SetValueHelper(this, BlockSizeParameter, ref this.blockSize, value);
        }

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

            using (SKPaint paint = new SKPaint() {Color = SKColors.Orange}) {
                rc.Canvas.DrawRect(new SKRect(0, 0, (float) this.BlockSize, (float) this.BlockSize), paint);
            }

            // int width = (int) this.renderSize.X;
            // int height = (int) this.renderSize.Y;
            // int blockSize = this.BlocKSize;
            // int rowBytes = width * rc.ImageInfo.BytesPerPixel;
            // IntPtr lpPixels = rc.Pixmap.GetPixels();
            //
            // for (int y = 0; y < height; y += blockSize) {
            //     for (int x = 0; x < width; x += blockSize) {
            //         int blockWidth = Math.Min(blockSize, width - x);
            //         int blockHeight = Math.Min(blockSize, height - y);
            //
            //         long totalR = 0, totalG = 0, totalB = 0, totalA = 0;
            //         for (int dy = 0; dy < blockHeight; dy++) {
            //             for (int dx = 0; dx < blockWidth; dx++) {
            //                 int pixelOffset = (y + dy) * rowBytes + (x + dx) * 4;
            //                 int pixel = Marshal.ReadInt32(lpPixels + pixelOffset);
            //                 SKColor colour = new SKColor((uint) pixel);
            //
            //                 totalB += colour.Blue;
            //                 totalG += colour.Green;
            //                 totalR += colour.Red;
            //                 totalA += colour.Alpha;
            //             }
            //         }
            //
            //         byte avgB = (byte) (totalB / (blockWidth * blockHeight));
            //         byte avgG = (byte) (totalG / (blockWidth * blockHeight));
            //         byte avgR = (byte) (totalR / (blockWidth * blockHeight));
            //         byte avgA = (byte) (totalA / (blockWidth * blockHeight));
            //
            //         for (int dy = 0; dy < blockHeight; dy++) {
            //             for (int dx = 0; dx < blockWidth; dx++) {
            //                 int pixelOffset = (y + dy) * rowBytes + (x + dx) * 4;
            //                 int pixel = avgB | (avgG << 8) | (avgR << 16) | (avgA << 24);
            //                 Marshal.WriteInt32(lpPixels, pixelOffset, pixel);
            //                 // Marshal.WriteByte(lpPixels, pixelOffset, avgB);
            //                 // Marshal.WriteByte(lpPixels, pixelOffset + 1, avgG);
            //                 // Marshal.WriteByte(lpPixels, pixelOffset + 2, avgR);
            //                 // Marshal.WriteByte(lpPixels, pixelOffset + 3, avgA);
            //             }
            //         }
            //     }
            // }
        }
    }
}