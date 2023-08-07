using System;
using System.Threading;
using FFmpeg.AutoGen;
using MediaStream = FramePFX.Core.FFmpegWrapper.Containers.MediaStream;

namespace FramePFX.Core.FFmpeg
{
    /// <summary>
    /// A thread-based FFmpeg decoder
    /// </summary>
    public class FFmpegDecodeThread : IDisposable
    {
        private long lastFrame;
        private readonly string filePath;

        // Demuxer
        private unsafe AVFormatContext* _ctx;
        private MediaStream[] streams;

        private readonly object getFrameMutex;
        private readonly Thread thread;

        private volatile bool stop;

        public FFmpegDecodeThread(string filePath)
        {
            this.filePath = filePath;
            this.getFrameMutex = new object();
            this.thread = new Thread(this.ThreadMain);
        }

        private void ThreadMain()
        {
            while (!this.stop)
            {
                Thread.Sleep(1);
            }
        }

        public void Dispose()
        {
        }
    }
}