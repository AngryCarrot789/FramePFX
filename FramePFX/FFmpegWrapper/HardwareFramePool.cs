using System;
using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper {
    public unsafe class HardwareFramePool : FFObject {
        private AVBufferRef* _ctx;

        public AVBufferRef* Handle {
            get {
                this.ValidateNotDisposed();
                return this._ctx;
            }
        }

        public AVHWFramesContext* RawHandle {
            get {
                this.ValidateNotDisposed();
                return (AVHWFramesContext*) this._ctx->data;
            }
        }

        public int Width => this.RawHandle->width;
        public int Height => this.RawHandle->height;
        public AVPixelFormat HwFormat => this.RawHandle->format;
        public AVPixelFormat SwFormat => this.RawHandle->sw_format;

        public HardwareFramePool(AVBufferRef* deviceCtx) {
            this._ctx = deviceCtx;
        }

        public VideoFrame AllocFrame() {
            AVFrame* frame = ffmpeg.av_frame_alloc();
            int err = ffmpeg.av_hwframe_get_buffer(this._ctx, frame, 0);
            if (err < 0) {
                ffmpeg.av_frame_free(&frame);
                throw FFUtils.GetException(err, "Failed to allocate hardware frame");
            }

            return new VideoFrame(frame, takeOwnership: true);
        }

        protected override void Free() {
            if (this._ctx != null) {
                fixed (AVBufferRef** ppCtx = &this._ctx) {
                    ffmpeg.av_buffer_unref(ppCtx);
                }
            }
        }

        private void ValidateNotDisposed() {
            if (this._ctx == null) {
                throw new ObjectDisposedException(nameof(HardwareFramePool));
            }
        }
    }
}