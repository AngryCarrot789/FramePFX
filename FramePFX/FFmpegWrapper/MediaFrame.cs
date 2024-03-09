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
    public abstract unsafe class MediaFrame : FFObject
    {
        internal AVFrame* frame;
        protected bool _ownsFrame = true;

        public AVFrame* Handle {
            get
            {
                this.ValidateNotDisposed();
                return this.frame;
            }
        }

        public long? BestEffortTimestamp => FFUtils.GetPTS(this.frame->best_effort_timestamp);

        public long? PresentationTimestamp {
            get => FFUtils.GetPTS(this.frame->pts);
            set => FFUtils.SetPTS(ref this.frame->pts, value);
        }

        protected override void Free()
        {
            if (this.frame != null && this._ownsFrame)
            {
                fixed (AVFrame** ppFrame = &this.frame)
                {
                    ffmpeg.av_frame_free(ppFrame);
                }
            }

            this.frame = null;
        }

        protected void ValidateNotDisposed()
        {
            if (this.frame == null)
            {
                throw new ObjectDisposedException(nameof(MediaFrame));
            }
        }
    }
}