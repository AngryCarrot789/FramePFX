using System;
using FFmpeg.AutoGen;
using FramePFX.FFmpegWrapper;
using FramePFX.FFmpegWrapper.Codecs;
using FramePFX.FFmpegWrapper.Containers;

namespace FramePFX.FFmpeg
{
    public class VideoStream : StreamWrapper
    {
        private VideoDecoder decoder;
        private FrameQueue queue;

        protected override MediaDecoder DecoderInternal => this.decoder;

        public VideoStream(MediaStream stream) : base(stream)
        {
        }

        public unsafe VideoDecoder GetDecoder(bool open = true, int bufferedFrames = 0)
        {
            if (this.decoder == null || !this.decoder.IsOpen)
            {
                AVCodecID codecId = this.Stream.Handle->codecpar->codec_id;
                this.decoder = new VideoDecoder(codecId);
                int err = ffmpeg.avcodec_parameters_to_context(this.decoder.Handle, this.Stream.Handle->codecpar);
                if (FFUtils.GetException(err, "Could not copy stream parameters to the video decoder.", out Exception e))
                {
                    this.decoder.Dispose();
                    this.decoder = null;
                    throw e;
                }

                if (open)
                {
                    try
                    {
                        this.decoder.Open();
                    }
                    catch
                    {
                        this.decoder.Dispose();
                        this.decoder = null;
                        throw;
                    }
                }
            }

            if (bufferedFrames > 0)
            {
                this.queue = new FrameQueue(this.Stream, bufferedFrames);
            }

            return this.decoder;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.queue?.Dispose();
            this.queue = null;
        }

        public override void DisposeDecoder(bool flushBuffers = true)
        {
            base.DisposeDecoder(flushBuffers);
            this.decoder = null;
        }
    }
}