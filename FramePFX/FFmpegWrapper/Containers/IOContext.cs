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

using System;
using System.IO;
using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper.Containers
{
    public abstract unsafe class IOContext : FFObject
    {
        private AVIOContext* _ctx;

        public AVIOContext* Handle {
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