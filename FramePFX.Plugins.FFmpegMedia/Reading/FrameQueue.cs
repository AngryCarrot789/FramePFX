// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using FFmpeg.AutoGen;
using FramePFX.Plugins.FFmpegMedia.Wrappers;
using FramePFX.Plugins.FFmpegMedia.Wrappers.Containers;

namespace FramePFX.Plugins.FFmpegMedia.Reading;

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