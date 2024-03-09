//
// MIT License
//
// Copyright (c) 2023 dubiousconst282
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper
{
    public unsafe class HardwareFramePool : FFObject
    {
        private AVBufferRef* _ctx;

        public AVBufferRef* Handle {
            get
            {
                this.ValidateNotDisposed();
                return this._ctx;
            }
        }

        public AVHWFramesContext* RawHandle {
            get
            {
                this.ValidateNotDisposed();
                return (AVHWFramesContext*) this._ctx->data;
            }
        }

        public int Width => this.RawHandle->width;
        public int Height => this.RawHandle->height;
        public AVPixelFormat HwFormat => this.RawHandle->format;
        public AVPixelFormat SwFormat => this.RawHandle->sw_format;

        public HardwareFramePool(AVBufferRef* deviceCtx)
        {
            this._ctx = deviceCtx;
        }

        public VideoFrame AllocFrame()
        {
            AVFrame* frame = ffmpeg.av_frame_alloc();
            int err = ffmpeg.av_hwframe_get_buffer(this._ctx, frame, 0);
            if (err < 0)
            {
                ffmpeg.av_frame_free(&frame);
                throw FFUtils.GetException(err, "Failed to allocate hardware frame");
            }

            return new VideoFrame(frame, takeOwnership: true);
        }

        protected override void Free()
        {
            if (this._ctx != null)
            {
                fixed (AVBufferRef** ppCtx = &this._ctx)
                {
                    ffmpeg.av_buffer_unref(ppCtx);
                }
            }
        }

        private void ValidateNotDisposed()
        {
            if (this._ctx == null)
            {
                throw new ObjectDisposedException(nameof(HardwareFramePool));
            }
        }
    }
}