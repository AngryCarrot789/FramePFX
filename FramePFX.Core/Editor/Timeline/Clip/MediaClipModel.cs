using System;
using System.Numerics;
using System.Threading.Tasks;
using FFmpeg.Wrapper;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.Clip {
    public class MediaClipModel : BaseResourceClip<ResourceMedia> {
        private VideoFrame frameRgb, downloadedHwFrame;
        private SwScaler scaler;
        private long targetFrame;
        private long currentFrameNo = -1;
        private TimeSpan frameTimestamp = TimeSpan.Zero;
        private Task catchUpTask;
        private VideoFrame readyFrame;

        public MediaClipModel() {
        }

        public override Vector2 GetSize() {
            if (this.TryGetResource(out ResourceMedia resource))
                return resource.GetResolution();
            return Vector2.Zero;
        }

        public override void Render(RenderContext render, long frame, SKColorFilter alphaFilter) {
            if (!this.TryGetResource(out ResourceMedia resource)) {
                return;
            }

            if (frame != this.currentFrameNo) {
                // this.EnsureTaskRunning();
                this.ResyncFrame(frame, render, resource);
                this.currentFrameNo = frame;
                this.targetFrame = frame;
            }

            if (this.readyFrame != null) {
                this.Transform(render.Canvas, out var rect);
                this.UploadFrame(this.readyFrame, render);
            }
        }

        protected override VideoClipModel NewInstance() {
            return new MediaClipModel();
        }

        private void CreateTexture(RenderContext render, Resolution resolution) {
            //Select the smallest size from either clip or source for our temp frames
            int frameW = resolution.Width;
            int frameH = resolution.Height;
            this.frameRgb = new VideoFrame(frameW, frameH, PixelFormats.RGBA);
        }

        private void ResyncFrame(long frameNo, RenderContext render, ResourceMedia media) {
            if (this.frameRgb == null) {
                this.CreateTexture(render, media.GetResolution());
                return;
            }

            double timeScale = this.Project.Settings.FrameRate;
            TimeSpan timestamp = TimeSpan.FromSeconds((frameNo - this.FrameBegin + this.MediaFrameOffset) / timeScale);
            this.readyFrame = media.GetFrameAt(timestamp);
        }

        private void UploadFrame(VideoFrame frame, RenderContext render) {
            if (frame.IsHardwareFrame) {
                // As of ffmpeg 6.0, GetHardwareTransferFormats() only returns more than one format for VAAPI,
                // which isn't widely supported on Windows yet, so we can't transfer directly to RGB without
                // hacking into the API specific device context (like D3D11VA).
                frame.TransferTo(this.downloadedHwFrame ?? (this.downloadedHwFrame = new VideoFrame()));
                frame = this.downloadedHwFrame;
            }

            if (this.scaler == null) {
                this.scaler = new SwScaler(frame.Format, this.frameRgb.Format);
            }

            this.scaler.Convert(frame, this.frameRgb);
            unsafe {
                Span<byte> pixelData = this.frameRgb.GetPlaneSpan<byte>(0, out int rowBytes);
                fixed (byte* ptr = pixelData) {
                    SKImageInfo image = new SKImageInfo(this.frameRgb.Width, this.frameRgb.Height, SKColorType.Rgba8888);
                    using (SKImage img = SKImage.FromPixels(image, (IntPtr) ptr, rowBytes)) {
                        render.Canvas.DrawImage(img, 0, 0, null);
                    }
                }
            }
        }
    }
}