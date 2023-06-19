using System;
using System.Numerics;
using FFmpeg.Wrapper;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.VideoClips {
    public class MediaClipModel : BaseResourceClip<ResourceMedia> {
        private VideoFrame renderFrameRgb, downloadedHwFrame;
        private SwScaler scaler;
        private VideoFrame readyFrame;
        private long currentFrame = -1;

        public MediaClipModel() {
        }

        public override Vector2? GetSize() {
            if (this.TryGetResource(out ResourceMedia resource))
                return resource.GetResolution();
            return null;
        }

        public override void Render(RenderContext render, long frame) {
            if (frame != this.currentFrame) {
                if (!this.TryGetResource(out ResourceMedia resource))
                    return;
                if (resource.Demuxer == null)
                    resource.OpenMediaFromFile();
                this.ResyncFrame(frame, resource);
                this.currentFrame = frame;
            }

            if (this.readyFrame != null) {
                this.Transform(render);
                this.UploadFrame(this.readyFrame, render);
            }
        }

        protected override ClipModel NewInstance() {
            return new MediaClipModel();
        }

        protected override void OnResourceChanged(ResourceMedia oldItem, ResourceMedia newItem) {
            base.OnResourceChanged(oldItem, newItem);
            this.renderFrameRgb?.Dispose();
            this.renderFrameRgb = null;
            this.downloadedHwFrame?.Dispose();
            this.downloadedHwFrame = null;
            this.scaler?.Dispose();
            this.scaler = null;
        }

        private void ResyncFrame(long frame, ResourceMedia media) {
            if (this.renderFrameRgb == null) {
                Resolution resolution = media.GetResolution();
                this.renderFrameRgb = new VideoFrame(resolution.Width, resolution.Height, PixelFormats.RGBA);
            }

            double timeScale = this.Project.Settings.FrameRate;
            TimeSpan timestamp = TimeSpan.FromSeconds((frame - this.FrameBegin + this.MediaFrameOffset) / timeScale);
            // No need to dispose as the frames are stored in a frame buffer, which is disposed by the resource itself
            this.readyFrame = media.GetFrameAt(timestamp);
        }

        // TODO: Maybe add an async frame fetcher that buffers the frames, or maybe add
        // a project preview resolution so that decoding is lightning fast for low resolution?

        private void UploadFrame(VideoFrame frame, RenderContext render) {
            if (frame.IsHardwareFrame) {
                // As of ffmpeg 6.0, GetHardwareTransferFormats() only returns more than one format for VAAPI,
                // which isn't widely supported on Windows yet, so we can't transfer directly to RGB without
                // hacking into the API specific device context (like D3D11VA).
                frame.TransferTo(this.downloadedHwFrame ?? (this.downloadedHwFrame = new VideoFrame()));
                frame = this.downloadedHwFrame;
            }

            if (this.scaler == null) {
                this.scaler = new SwScaler(frame.Format, this.renderFrameRgb.Format);
            }

            this.scaler.Convert(frame, this.renderFrameRgb);
            unsafe {
                Span<byte> pixelData = this.renderFrameRgb.GetPlaneSpan<byte>(0, out int rowBytes);
                fixed (byte* ptr = pixelData) {
                    SKImageInfo image = new SKImageInfo(this.renderFrameRgb.Width, this.renderFrameRgb.Height, SKColorType.Rgba8888);
                    using (SKImage img = SKImage.FromPixels(image, (IntPtr) ptr, rowBytes)) {
                        render.Canvas.DrawImage(img, 0, 0, null);
                    }
                }
            }
        }
    }
}