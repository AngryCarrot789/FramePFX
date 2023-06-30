using System;
using System.Numerics;
using FFmpeg.AutoGen;
using FFmpeg.Wrapper;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Rendering;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timelines.VideoClips {
    public class MediaClip : BaseResourceClip<ResourceMedia> {
        private VideoFrame renderFrameRgb, downloadedHwFrame;
        private SwScaler scaler;
        private VideoFrame readyFrame;
        private long currentFrame = -1;

        // TODO: decoder thread
        // public override bool UseAsyncRendering => true;

        public MediaClip() {
        }

        public override Vector2? GetSize() {
            return (Vector2?) (this.TryGetResource(out ResourceMedia resource) ? resource.GetResolution() : null);
        }

        public static IntPtr AddressOf(ref int value) {
            return (IntPtr) value;
        }

        public static unsafe void GetFrameData(VideoFrame frame, int plane, byte** data, out int stride) {
            int height = frame.GetPlaneSize(plane).Height;
            AVFrame* ptr = frame.Handle;
            *data = ptr->data[(uint) plane];
            int rowSize = ptr->linesize[(uint) plane];

            if (rowSize < 0) {
                *data += rowSize * (height - 1);
                rowSize = unchecked(rowSize * -1);
            }

            stride = rowSize / sizeof(byte);
        }

        public override void Render(RenderContext rc, long frame) {
            if (!this.TryGetResource(out ResourceMedia resource))
                return;

            if (frame != this.currentFrame || this.renderFrameRgb == null || resource.Demuxer == null) {
                if (resource.Demuxer == null || resource.stream == null)
                    resource.OpenMediaFromFile();
                if (this.renderFrameRgb == null) {
                    unsafe {
                        AVCodecParameters* pars = resource.stream.Handle->codecpar;
                        this.renderFrameRgb = new VideoFrame(pars->width, pars->height, PixelFormats.RGBA);
                    }
                }

                double timeScale = this.Project.Settings.FrameRate.AsDouble;
                TimeSpan timestamp = TimeSpan.FromSeconds((frame - this.FrameBegin + this.MediaFrameOffset) / timeScale);
                // No need to dispose as the frames are stored in a frame buffer, which is disposed by the resource itself
                this.readyFrame = resource.GetFrameAt(timestamp);
                this.currentFrame = frame;
            }

            if (this.readyFrame != null) {
                VideoFrame ready = this.readyFrame;
                // TODO: Maybe add an async frame fetcher that buffers the frames, or maybe add
                // a project preview resolution so that decoding is lightning fast for low resolution?

                if (this.renderFrameRgb == null) {
                    return;
                }

                this.Transform(rc);
                if (ready.IsHardwareFrame) {
                    // As of ffmpeg 6.0, GetHardwareTransferFormats() only returns more than one format for VAAPI,
                    // which isn't widely supported on Windows yet, so we can't transfer directly to RGB without
                    // hacking into the API specific device context (like D3D11VA).
                    ready.TransferTo(this.downloadedHwFrame ?? (this.downloadedHwFrame = new VideoFrame()));
                    ready = this.downloadedHwFrame;
                }

                if (this.scaler == null) {
                    this.scaler = new SwScaler(ready.Format, this.renderFrameRgb.Format);
                }

                this.scaler.Convert(ready, this.renderFrameRgb);
                unsafe {
                    byte* ptr;
                    GetFrameData(this.renderFrameRgb, 0, &ptr, out int rowBytes);
                    SKImageInfo image = new SKImageInfo(this.renderFrameRgb.Width, this.renderFrameRgb.Height, SKColorType.Rgba8888);
                    using (SKImage img = SKImage.FromPixels(image, (IntPtr) ptr, rowBytes)) {
                        rc.Canvas.DrawImage(img, 0, 0);
                    }
                }
            }
        }

        protected override Clip NewInstance() {
            return new MediaClip();
        }

        protected override void OnResourceChanged(ResourceMedia oldItem, ResourceMedia newItem) {
            this.renderFrameRgb?.Dispose();
            this.renderFrameRgb = null;
            this.downloadedHwFrame?.Dispose();
            this.downloadedHwFrame = null;
            this.scaler?.Dispose();
            this.scaler = null;
            base.OnResourceChanged(oldItem, newItem);
        }
    }
}