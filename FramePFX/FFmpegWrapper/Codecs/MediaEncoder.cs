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

namespace FramePFX.FFmpegWrapper.Codecs {
    public abstract unsafe class MediaEncoder : CodecBase {
        public int BitRate {
            get => (int) this.ctx->bit_rate;
            set => this.SetOrThrowIfOpen(ref this.ctx->bit_rate, value);
        }

        public MediaEncoder(AVCodecContext* ctx, AVMediaType expectedType, bool takeOwnership = true) : base(ctx, expectedType, takeOwnership) {
        }

        public void SetOption(string name, string value) {
            int tempQualifier = ffmpeg.av_opt_set(this.Handle->priv_data, name, value, 0);
            if (tempQualifier < 0 && tempQualifier != ffmpeg.EAGAIN && tempQualifier != ffmpeg.AVERROR_EOF) {
                throw FFUtils.GetException(tempQualifier);
            }

            int temp = tempQualifier;
        }

        public bool ReceivePacket(MediaPacket pkt) {
            LavResult result = (LavResult) ffmpeg.avcodec_receive_packet(this.Handle, pkt.Handle);
            if (result != LavResult.Success && result != LavResult.TryAgain && result != LavResult.EndOfFile) {
                result.ThrowIfError("Could not encode packet");
            }

            return result == 0;
        }

        public bool SendFrame(MediaFrame frame) {
            LavResult result = (LavResult) ffmpeg.avcodec_send_frame(this.Handle, frame == null ? null : frame.Handle);

            if (result != LavResult.Success && !(result == LavResult.EndOfFile && frame == null)) {
                result.ThrowIfError("Could not encode frame");
            }

            return result == 0;
        }

        /// <summary> Returns the correct <see cref="MediaFrame.PresentationTimestamp"/> for the given timestamp, in respect to <see cref="CodecBase.TimeBase"/>. </summary>
        public long GetFramePts(TimeSpan time) {
            return this.GetFramePts(time.Ticks, new AVRational() {num = 1, den = (int) TimeSpan.TicksPerSecond});
        }

        /// <summary> Rescales the given timestamp to be in terms of <see cref="CodecBase.TimeBase"/>. </summary>
        public long GetFramePts(long pts, AVRational timeBase) {
            return ffmpeg.av_rescale_q(pts, timeBase, this.TimeBase);
        }
    }
}