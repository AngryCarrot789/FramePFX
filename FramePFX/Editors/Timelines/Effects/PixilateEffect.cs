using System.Numerics;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Clips;

namespace FramePFX.Editors.Timelines.Effects {
    public class PixilateEffect : VideoEffect {
        public int BlocKSize = 4;
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

        public override void PostProcessFrame(RenderContext rc) {
            base.PostProcessFrame(rc);

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