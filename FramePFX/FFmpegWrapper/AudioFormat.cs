using System;
using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper {
    public unsafe readonly struct AudioFormat {
        public AVSampleFormat SampleFormat { get; }
        public int SampleRate { get; }
        public AVChannelLayout Layout { get; }

        public int NumChannels => this.Layout.nb_channels;

        public int BytesPerSample => ffmpeg.av_get_bytes_per_sample(this.SampleFormat);
        public bool IsPlanar => ffmpeg.av_sample_fmt_is_planar(this.SampleFormat) != 0;

        public AudioFormat(AVSampleFormat fmt, int sampleRate, int numChannels) {
            this.SampleFormat = fmt;
            this.SampleRate = sampleRate;

            AVChannelLayout tempLayout;
            ffmpeg.av_channel_layout_default(&tempLayout, numChannels);
            this.Layout = tempLayout;
        }

        public AudioFormat(AVSampleFormat fmt, int sampleRate, AVChannelLayout layout) {
            this.SampleFormat = fmt;
            this.SampleRate = sampleRate;
            this.Layout = layout;
        }

        public AudioFormat(AVCodecContext* ctx) {
            if (ctx->codec_type != AVMediaType.AVMEDIA_TYPE_AUDIO) {
                throw new ArgumentException("Codec context media type is not audio.", nameof(ctx));
            }

            this.SampleFormat = ctx->sample_fmt;
            this.SampleRate = ctx->sample_rate;
            this.Layout = ctx->ch_layout;
        }

        public AudioFormat(AVFrame* frame) {
            if (frame->ch_layout.nb_channels <= 0 || frame->sample_rate <= 0) {
                throw new ArgumentException("The frame does not specify a valid audio format.", nameof(frame));
            }

            this.SampleFormat = (AVSampleFormat) frame->format;
            this.SampleRate = frame->sample_rate;
            this.Layout = frame->ch_layout;
        }

        public override string ToString() {
            string fmt = this.SampleFormat.ToString().Substring("AV_SAMPLE_FMT_".Length);
            return $"{this.NumChannels}ch {fmt}, {this.SampleRate / 1000.0:0.0}KHz";
        }
    }
}