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

using System;
using System.Threading;
using FFmpeg.AutoGen;
using MediaStream = FramePFX.FFmpegWrapper.Containers.MediaStream;

namespace FramePFX.FFmpeg {
    /// <summary>
    /// A thread-based FFmpeg decoder
    /// </summary>
    public class FFmpegDecodeThread : IDisposable {
        private long lastFrame;
        private readonly string filePath;

        // Demuxer
        private unsafe AVFormatContext* _ctx;
        private MediaStream[] streams;

        private readonly object getFrameMutex;
        private readonly Thread thread;

        private volatile bool stop;

        public FFmpegDecodeThread(string filePath) {
            this.filePath = filePath;
            this.getFrameMutex = new object();
            this.thread = new Thread(this.ThreadMain);
        }

        private void ThreadMain() {
            while (!this.stop) {
                Thread.Sleep(1);
            }
        }

        public void Dispose() {
        }
    }
}