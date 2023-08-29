using System;
using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper.Containers {
    public unsafe class MediaStream {
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

        public MediaStream(AVStream* stream) {
            this.Handle = stream;
        }

        /// <summary> Returns the corresponding <see cref="TimeSpan"/> for the given timestamp based on <see cref="TimeBase"/> units. </summary>
        public TimeSpan GetTimestamp(long pts) => FFUtils.GetTimeSpan(pts, this.TimeBase) ?? TimeSpan.Zero;
    }
}