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

namespace FramePFX.Plugins.FFmpegMedia.Wrappers.Codecs;

public unsafe class AudioDecoder : MediaDecoder {
    public AVSampleFormat SampleFormat => this.ctx->sample_fmt;
    public int SampleRate => this.ctx->sample_rate;
    public int NumChannels => this.ctx->ch_layout.nb_channels;
    public AVChannelLayout ChannelLayout => this.ctx->ch_layout;

    public AudioFormat Format => new AudioFormat(this.ctx);

    public AudioDecoder(AVCodecID codecId) : this(FindCodecFromId(codecId, enc: false)) {
    }

    public AudioDecoder(AVCodec* codec) : this(AllocContext(codec)) {
    }

    public AudioDecoder(AVCodecContext* ctx, bool takeOwnership = true) : base(ctx, MediaTypes.Audio, takeOwnership) {
    }
}