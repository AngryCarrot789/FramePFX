using System;
using FFmpeg.AutoGen;
using FramePFX.FFmpegWrapper.Codecs;
using FramePFX.FFmpegWrapper.Containers;

namespace FramePFX.FFmpeg
{
    public abstract class StreamWrapper : IDisposable
    {
        public MediaStream Stream { get; }

        protected abstract MediaDecoder DecoderInternal { get; }

        protected StreamWrapper(MediaStream stream)
        {
            this.Stream = stream;
        }

        ~StreamWrapper() => this.Dispose(false);

        public virtual void DisposeDecoder(bool flushBuffers = true)
        {
            MediaDecoder decoder = this.DecoderInternal;
            if (decoder != null && (decoder.IsOpen || !decoder.IsDisposed))
            {
                if (flushBuffers)
                {
                    unsafe
                    {
                        ffmpeg.avcodec_flush_buffers(decoder.Handle);
                    }
                }

                decoder.Dispose();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.DisposeDecoder(disposing);
        }
    }
}