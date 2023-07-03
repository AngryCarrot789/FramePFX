using System;
using FFmpeg.AutoGen;

namespace FramePFX.Core.FFmpegWrapper.Codecs {

    public abstract unsafe class MediaEncoder : CodecBase {
        public int BitRate {
            get => (int) this.ctx->bit_rate;
            set => this.SetOrThrowIfOpen(ref this.ctx->bit_rate, value);
        }

        public MediaEncoder(AVCodecContext* ctx, AVMediaType expectedType, bool takeOwnership = true)
            : base(ctx, expectedType, takeOwnership) {
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