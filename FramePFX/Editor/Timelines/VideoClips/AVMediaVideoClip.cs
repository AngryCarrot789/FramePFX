using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.FFmpegWrapper;
using FramePFX.Rendering;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class AVMediaVideoClip : VideoClip {
        private VideoFrame renderFrameRgb, downloadedHwFrame;
        public unsafe SwsContext* scaler;
        private PictureFormat scalerInputFormat;
        private VideoFrame lastReadyFrame;
        private long currentFrame = -1;

        // TODO: decoder thread
        // public override bool UseAsyncRendering => true;

        public IResourcePathKey<ResourceAVMedia> ResourceAVMediaKey { get; }

        public override bool UseCustomOpacityCalculation => true;

        private Task<VideoFrame> GetFrameTask;

        public AVMediaVideoClip() {
            this.ResourceAVMediaKey = this.ResourceHelper.RegisterKeyByTypeName<ResourceAVMedia>();
            this.ResourceAVMediaKey.ResourceChanged += this.OnResourceChanged;
        }

        private bool TryGetResource(out ResourceAVMedia resource) => this.ResourceAVMediaKey.TryGetResource(out resource);

        public override Vector2? GetSize() {
            return (Vector2?) (this.TryGetResource(out ResourceAVMedia resource) ? resource.GetResolution() : null);
        }

        public override bool OnBeginRender(long frame) {
            if (!this.TryGetResource(out ResourceAVMedia resource))
                return false;
            if (resource.stream == null || resource.Demuxer == null)
                return false;

            if (frame != this.currentFrame || this.renderFrameRgb == null) {
                if (this.renderFrameRgb == null) {
                    unsafe {
                        AVCodecParameters* pars = resource.stream.Handle->codecpar;
                        this.renderFrameRgb = new VideoFrame(pars->width, pars->height, PixelFormats.RGBA);
                    }
                }

                double timeScale = this.Project.Settings.TimeBase.ToDouble;
                TimeSpan timestamp = TimeSpan.FromSeconds((frame - this.FrameBegin + this.MediaFrameOffset) / timeScale);
                // No need to dispose as the frames are stored in a frame buffer, which is disposed by the resource itself
                this.currentFrame = frame;
                this.GetFrameTask = Task.Run(async () => {
                    Task task = Interlocked.Exchange(ref resource.CurrentGetFrameTask, this.GetFrameTask);
                    if (task != null && !task.IsCompleted) {
                        await task;
                    }

                    VideoFrame ready = resource.GetFrameAt(timestamp);
                    if (ready != null && !ready.IsDisposed) {
                        // TODO: Maybe add an async frame fetcher that buffers the frames, or maybe add
                        // a project preview resolution so that decoding is lightning fast for low resolution?
                        if (ready.IsHardwareFrame) {
                            // As of ffmpeg 6.0, GetHardwareTransferFormats() only returns more than one format for VAAPI,
                            // which isn't widely supported on Windows yet, so we can't transfer directly to RGB without
                            // hacking into the API specific device context (like D3D11VA).
                            ready.TransferTo(this.downloadedHwFrame ?? (this.downloadedHwFrame = new VideoFrame()));
                            ready = this.downloadedHwFrame;
                        }

                        this.ScaleFrame(ready);
                        return ready;
                    }

                    return null;
                });
            }

            return true;
        }

        public override async Task OnEndRender(RenderContext rc, long frame) {
            VideoFrame ready;
            if (this.GetFrameTask != null) {
                this.lastReadyFrame = ready = await this.GetFrameTask;
            }
            else {
                ready = this.lastReadyFrame;
            }

            if (ready == null || ready.IsDisposed) {
                return;
            }

            unsafe {
                byte* ptr;
                GetFrameData(this.renderFrameRgb, 0, &ptr, out int rowBytes);
                SKImageInfo image = new SKImageInfo(this.renderFrameRgb.Width, this.renderFrameRgb.Height, SKColorType.Rgba8888);
                using (SKImage img = SKImage.FromPixels(image, (IntPtr) ptr, rowBytes)) {
                    if (img == null) {
                        return;
                    }

                    using (SKPaint paint = new SKPaint() {FilterQuality = rc.RenderFilterQuality, ColorF = new SKColorF(1f, 1f, 1f, (float) this.Opacity)}) {
                        rc.Canvas.DrawImage(img, 0, 0, paint);
                    }
                }
            }
        }

        private unsafe void ScaleFrame(VideoFrame ready) {
            if (this.scaler == null) {
                PictureFormat srcfmt = ready.Format;
                PictureFormat dstfmt = this.renderFrameRgb.Format;
                this.scalerInputFormat = srcfmt;
                this.scaler = ffmpeg.sws_getContext(srcfmt.Width, srcfmt.Height, srcfmt.PixelFormat, dstfmt.Width, dstfmt.Height, dstfmt.PixelFormat, ffmpeg.SWS_BICUBIC, null, null, null);
            }

            AVFrame* src = ready.Handle;
            AVFrame* dst = this.renderFrameRgb.Handle;
            ffmpeg.sws_scale(this.scaler, src->data, src->linesize, 0, this.scalerInputFormat.Height, dst->data, dst->linesize);
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

        protected override Clip NewInstanceForClone() {
            return new AVMediaVideoClip();
        }

        protected void OnResourceChanged(ResourceAVMedia oldItem, ResourceAVMedia newItem) {
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
        }

        // Help from https://github.com/Golim4r/OpenGL-Video-Player
        public unsafe class DecoderThread {
            private readonly Thread thread;
            public volatile bool isPaused;
            public volatile bool isRunning;

            public AVFormatContext* pFormatCtx;
            public int videoStream;
            public AVCodecContext* pCodecCtx;
            public AVFrame* pFrame;
            public AVFrame* pFrameRGB;
            public SwsContext* sws_ctx;

            public DecoderThread() {
                this.thread = new Thread(this.ThreadMain);
                this.isRunning = true;
                this.isPaused = true;
                this.thread.Start();
            }

            private void ThreadMain() {
                while (this.isRunning) {
                    while (this.isPaused) {
                        Thread.Sleep(10);
                    }

                    this.ReadFrame();
                }
            }

            public void ReadFrame() {
                AVPacket packet;
                if (ffmpeg.av_read_frame(this.pFormatCtx, &packet) < 0) {
                    this.isPaused = true;
                    return;
                }

                // Is this a packet from the video stream?
                if (packet.stream_index == this.videoStream) {
                    LavResult result = (LavResult) ffmpeg.avcodec_receive_frame(this.pCodecCtx, this.pFrame);
                    if (result != LavResult.Success && result != LavResult.EndOfFile) {
                        this.isPaused = true;
                        return;
                    }

                    // Convert the image from its native format to RGB
                    ffmpeg.sws_scale(this.sws_ctx, this.pFrame->data, this.pFrame->linesize, 0, this.pCodecCtx->height, this.pFrameRGB->data, this.pFrameRGB->linesize);
                }

                // Free the packet that was allocated by ffmpeg.av_read_frame
                AVPacket* pkt = &packet;
                ffmpeg.av_packet_free(&pkt);
            }
        }
    }
}