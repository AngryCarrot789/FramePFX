using System;
using System.Numerics;
using FFmpeg.AutoGen;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.FFmpegWrapper;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timelines.VideoClips {
    public class OldMediaVideoClip : BaseResourceVideoClip<ResourceOldMedia> {
        private VideoFrame renderFrameRgb, downloadedHwFrame;
        public unsafe SwsContext* scaler;
        private PictureFormat scalerInputFormat;
        private PictureFormat scalerOutputFormat;
        private VideoFrame readyFrame;
        private long currentFrame = -1;

        // TODO: decoder thread
        // public override bool UseAsyncRendering => true;

        public OldMediaVideoClip() {
        }

        public override Vector2? GetSize() {
            return (Vector2?) (this.TryGetResource(out ResourceOldMedia resource) ? resource.GetResolution() : null);
        }

        public override void Render(RenderContext rc, long frame) {
            if (!this.TryGetResource(out ResourceOldMedia resource))
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

                double timeScale = this.Project.Settings.TimeBase.ToDouble;
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

                unsafe {
                    if (this.scaler == null) {
                        PictureFormat srcfmt = ready.Format;
                        PictureFormat dstfmt = this.renderFrameRgb.Format;
                        this.scalerInputFormat = srcfmt;
                        this.scalerOutputFormat = dstfmt;
                        this.scaler = ffmpeg.sws_getContext(srcfmt.Width, srcfmt.Height, srcfmt.PixelFormat, dstfmt.Width, dstfmt.Height, dstfmt.PixelFormat, ffmpeg.SWS_BICUBIC, null, null, null);
                    }

                    AVFrame* src = ready.Handle;
                    AVFrame* dst = this.renderFrameRgb.Handle;
                    ffmpeg.sws_scale(this.scaler, src->data, src->linesize, 0, this.scalerInputFormat.Height, dst->data, dst->linesize);

                    byte* ptr;
                    GetFrameData(this.renderFrameRgb, 0, &ptr, out int rowBytes);
                    SKImageInfo image = new SKImageInfo(this.renderFrameRgb.Width, this.renderFrameRgb.Height, SKColorType.Rgba8888);
                    using (SKImage img = SKImage.FromPixels(image, (IntPtr) ptr, rowBytes)) {
                        rc.Canvas.DrawImage(img, 0, 0);
                    }
                }
            }
        }

        public static unsafe (int, int) GetPlaneSize(VideoFrame frame, int plane) {
            (int x, int y) size = (frame.Width, frame.Height);

            //https://github.com/FFmpeg/FFmpeg/blob/c558fcf41e2027a1096d00b286954da2cc4ae73f/libavutil/imgutils.c#L111
            if (plane == 0) {
                return size;
            }

            AVPixFmtDescriptor* desc = ffmpeg.av_pix_fmt_desc_get(frame.PixelFormat);
            if (desc == null || (desc->flags & ffmpeg.AV_PIX_FMT_FLAG_HWACCEL) != 0) {
                throw new InvalidOperationException();
            }

            for (uint i = 0; i < 4; i++) {
                if (desc->comp[i].plane != plane)
                    continue;
                if ((i == 1 || i == 2) && (desc->flags & ffmpeg.AV_PIX_FMT_FLAG_RGB) == 0) {
                    size.x = Maths.CeilShr(size.x, desc->log2_chroma_w);
                    size.y = Maths.CeilShr(size.y, desc->log2_chroma_h);
                }

                return size;
            }

            throw new Exception("Could not get plane size");
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

        private static unsafe bool IsFormatEqual(AVFrame* src, AVFrame* dst, PictureFormat srcFmt, PictureFormat dstFmt) {
            return src->format == (int) srcFmt.PixelFormat &&
                   src->width == srcFmt.Width &&
                   src->height == srcFmt.Height &&
                   dst->format == (int) dstFmt.PixelFormat &&
                   dst->width == dstFmt.Width &&
                   dst->height == dstFmt.Height;
        }

        protected override Clip NewInstance() {
            return new OldMediaVideoClip();
        }

        protected override void OnResourceChanged(ResourceOldMedia oldItem, ResourceOldMedia newItem) {
            this.renderFrameRgb?.Dispose();
            this.renderFrameRgb = null;
            this.downloadedHwFrame?.Dispose();
            this.downloadedHwFrame = null;
            unsafe {
                if (this.scaler != null) {
                    ffmpeg.sws_freeContext(this.scaler);
                    this.scaler = null;
                }
            }

            base.OnResourceChanged(oldItem, newItem);
        }
    }
}