using FFmpeg.AutoGen;

namespace FramePFX.Core.FFmpegWrapper.Codecs {

    public unsafe class AudioDecoder : MediaDecoder {
        public AVSampleFormat SampleFormat => this.ctx->sample_fmt;
        public int SampleRate => this.ctx->sample_rate;
        public int NumChannels => this.ctx->ch_layout.nb_channels;
        public AVChannelLayout ChannelLayout => this.ctx->ch_layout;

        public AudioFormat Format => new AudioFormat(this.ctx);

        public AudioDecoder(AVCodecID codecId)
            : this(FindCodecFromId(codecId, enc: false)) {
        }

        public AudioDecoder(AVCodec* codec)
            : this(AllocContext(codec)) {
        }

        public AudioDecoder(AVCodecContext* ctx, bool takeOwnership = true)
            : base(ctx, MediaTypes.Audio, takeOwnership) {
        }
    }
}