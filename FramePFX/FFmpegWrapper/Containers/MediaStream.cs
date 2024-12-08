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

namespace FramePFX.FFmpegWrapper.Containers;

public unsafe class MediaStream
{
    public AVStream* Handle { get; }

    public int Index => this.Handle->index;

    public AVMediaType Type => this.Handle->codecpar->codec_type;

    /// <summary> The fundamental unit of time (in seconds) in terms of which frame timestamps are represented. </summary>
    public AVRational TimeBase => this.Handle->time_base;

    /// <summary> Pts of the first frame of the stream in presentation order, in stream time base. </summary>
    public long? StartTime => FFUtils.GetPTS(this.Handle->start_time);

    /// <summary> Decoding: duration of the stream, in stream time base. If a source file does not specify a duration, but does specify a bitrate, this value will be estimated from bitrate and file size. </summary>
    public TimeSpan? Duration => FFUtils.GetTimeSpan(this.Handle->duration, this.TimeBase);

    public double AvgFrameRate => ffmpeg.av_q2d(this.Handle->avg_frame_rate);

    public MediaStream(AVStream* stream)
    {
        this.Handle = stream;
    }

    /// <summary> Returns the corresponding <see cref="TimeSpan"/> for the given timestamp based on <see cref="TimeBase"/> units. </summary>
    public TimeSpan GetTimestamp(long pts) => FFUtils.GetTimeSpan(pts, this.TimeBase) ?? TimeSpan.Zero;
}