using System;
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

        public TimeSpan? Shift() {
            VideoFrame lastFrame = this.Current();
            this.index++;
            return this.GetTime(lastFrame);
        }

        public TimeSpan? GetTime(VideoFrame frame) {
            return frame.PresentationTimestamp is long pts ? this.stream.GetTimestamp(pts) : (TimeSpan?) null;
        }

        public VideoFrame GetNearest(TimeSpan timestamp, out double nearestDist) {
            VideoFrame nearestFrame = null;
            nearestDist = double.PositiveInfinity;
            foreach (VideoFrame frame in this.frames) {
                if (frame.PresentationTimestamp is long pts) {
                    double dist = (timestamp - this.stream.GetTimestamp(pts)).TotalSeconds;
                    if (Math.Abs(dist) < Math.Abs(nearestDist)) {
                        nearestFrame = frame;
                        nearestDist = dist;
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