using System;
using FFmpeg.AutoGen;
using FramePFX.FFmpegWrapper;
using FramePFX.FFmpegWrapper.Containers;

namespace FramePFX.FFmpeg {
    /// <summary>
    /// Rolling circular-buffer of VideoFrames
    /// </summary>
    public class FrameQueue : IDisposable {
        private readonly MediaStream stream;
        private readonly VideoFrame[] frames;
        private int index;

        public FrameQueue(MediaStream stream, int size) {
            this.stream = stream;
            this.frames = new VideoFrame[size];
            for (int i = 0; i < size; i++) {
                this.frames[i] = new VideoFrame();
            }
        }

        public VideoFrame Current() {
            return this.frames[this.index % this.frames.Length];
        }

        public bool Shift(out long spanTicks) {
            VideoFrame lastFrame = this.Current();
            this.index++;
            return this.GetFramePresentationTime(lastFrame, out spanTicks);
        }

        public unsafe bool GetFramePresentationTime(VideoFrame frame, out long spanTicks) {
            long pts = frame.frame->pts;
            if (pts == ffmpeg.AV_NOPTS_VALUE) {
                spanTicks = 0;
                return false;
            }
            else {
                spanTicks = ffmpeg.av_rescale_q(pts, this.stream.TimeBase, FFUtils.TimeSpanRational);
                return true;
            }
        }

        public VideoFrame GetNearest(TimeSpan timestamp, out double minDistToTimeStampTicks) {
            VideoFrame nearestFrame = null;
            minDistToTimeStampTicks = double.PositiveInfinity;
            foreach (VideoFrame frame in this.frames) {
                if (this.GetFramePresentationTime(frame, out long ticks)) {
                    double distance = new TimeSpan(timestamp.Ticks - ticks).TotalSeconds;
                    if (Math.Abs(distance) < Math.Abs(minDistToTimeStampTicks)) {
                        nearestFrame = frame;
                        minDistToTimeStampTicks = distance;
                    }
                }
            }

            return nearestFrame;
        }

        public void Dispose() {
            foreach (VideoFrame frame in this.frames) {
                frame.Dispose();
            }
        }
    }
}