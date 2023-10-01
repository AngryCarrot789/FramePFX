using System;
using System.IO;
using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper.Containers
{
    public abstract unsafe class IOContext : FFObject
    {
        private AVIOContext* _ctx;

        public AVIOContext* Handle
        {
            get
            {
                this.ValidateNotDisposed();
                return this._ctx;
            }
        }

        public bool CanRead { get; }
        public bool CanWrite { get; }
        public bool CanSeek => this._ctx->seekable != 0;

        //Keep lambda refs to prevent them from being GC collected
        private avio_alloc_context_read_packet _readFn;
        private avio_alloc_context_write_packet _writeFn;
        private avio_alloc_context_seek _seekFn;

        public IOContext(int bufferSize, bool canRead, bool canWrite, bool canSeek)
        {
            byte* buffer = (byte*) ffmpeg.av_mallocz((ulong) bufferSize);
            if (canRead)
                this._readFn = this.ReadBridge;
            if (canWrite)
                this._writeFn = this.WriteBridge;
            if (canSeek)
                this._seekFn = this.SeekBridge;

            this._ctx = ffmpeg.avio_alloc_context(
                buffer, bufferSize, canWrite ? 1 : 0, null, this._readFn, this._writeFn, this._seekFn
            );
        }

        private int ReadBridge(void* opaque, byte* buffer, int length)
        {
            int bytesRead = this.Read(new Span<byte>(buffer, length));
            return bytesRead > 0 ? bytesRead : ffmpeg.AVERROR_EOF;
        }

        private int WriteBridge(void* opaque, byte* buffer, int length)
        {
            this.Write(new ReadOnlySpan<byte>(buffer, length));
            return length;
        }

        private long SeekBridge(void* opaque, long offset, int whence)
        {
            if (whence == ffmpeg.AVSEEK_SIZE)
            {
                return this.GetLength() ?? ffmpeg.AVERROR(38); //ENOSYS
            }

            return this.Seek(offset, (SeekOrigin) whence);
        }

        /// <summary> Reads data from the underlying stream to <paramref name="buffer"/>. </summary>
        /// <returns>The number of bytes read. </returns>
        protected abstract int Read(Span<byte> buffer);

        /// <summary> Writes data to the underlying stream. </summary>
        protected abstract void Write(ReadOnlySpan<byte> buffer);

        /// <summary> Sets the position of the underlying stream. </summary>
        protected abstract long Seek(long offset, SeekOrigin origin);

        protected virtual long? GetLength() => null;

        protected override void Free()
        {
            if (this._ctx != null)
            {
                ffmpeg.av_free(this._ctx->buffer);
                fixed (AVIOContext** c = &this._ctx)
                    ffmpeg.avio_context_free(c);
            }
        }

        protected void ValidateNotDisposed()
        {
            if (this._ctx == null)
            {
                throw new ObjectDisposedException(nameof(IOContext));
            }
        }
    }
}