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

using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper;

public readonly unsafe struct AudioFormat {
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