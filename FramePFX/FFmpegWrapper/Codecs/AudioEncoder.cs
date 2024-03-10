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

namespace FramePFX.FFmpegWrapper.Codecs
{
    public unsafe class AudioEncoder : MediaEncoder
    {
        public AVSampleFormat SampleFormat
        {
            get => this.ctx->sample_fmt;
            set => this.SetOrThrowIfOpen(ref this.ctx->sample_fmt, value);
        }

        public int SampleRate
        {
            get => this.ctx->sample_rate;
            set => this.SetOrThrowIfOpen(ref this.ctx->sample_rate, value);
        }

        public int NumChannels => this.ctx->ch_layout.nb_channels;

        public AVChannelLayout ChannelLayout
        {
            get => this.ctx->ch_layout;
            set => this.SetOrThrowIfOpen(ref this.ctx->ch_layout, value);
        }

        public AudioFormat Format
        {
            get => new AudioFormat(this.ctx);
            set
            {
                this.ctx->sample_rate = value.SampleRate;
                this.ctx->sample_fmt = value.SampleFormat;
                this.ctx->ch_layout = value.Layout;
            }
        }

        /// <summary> Number of samples per channel in an audio frame (set after the encoder is opened). </summary>
        /// <remarks>
        /// Each submitted frame except the last must contain exactly frame_size samples per channel.
        /// May be null when the codec has AV_CODEC_CAP_VARIABLE_FRAME_SIZE set, then the frame size is not restricted.
        /// </remarks>
        public int? FrameSize => this.ctx->frame_size == 0 ? (int?) null : this.ctx->frame_size;

        public ReadOnlySpan<AVSampleFormat> SupportedSampleFormats
            => FFUtils.GetSpanFromSentinelTerminatedPtr(this.ctx->codec->sample_fmts, AVSampleFormat.AV_SAMPLE_FMT_NONE);

        public ReadOnlySpan<int> SupportedSampleRates
            => FFUtils.GetSpanFromSentinelTerminatedPtr(this.ctx->codec->supported_samplerates, 0);

        public AudioEncoder(AVCodecID codecId, in AudioFormat format, int bitrate) : this(FindCodecFromId(codecId, enc: true), format, bitrate)
        {
        }

        public AudioEncoder(AVCodec* codec, in AudioFormat format, int bitrate) : this(AllocContext(codec))
        {
            this.Format = format;
            this.BitRate = bitrate;
            this.TimeBase = new AVRational() {den = format.SampleRate, num = 1};
        }

        public AudioEncoder(AVCodecContext* ctx, bool takeOwnership = true) : base(ctx, MediaTypes.Audio, takeOwnership)
        {
        }
    }
}