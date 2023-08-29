using System;
using System.IO;
using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper.Containers {
    /// <summary> Wraps a <see cref="Stream"/> into a <see cref="AVIOContext"/>. </summary>
    public class StreamIOContext : IOContext {
        public Stream BaseStream { get; }

        private bool _leaveOpen;

        public StreamIOContext(Stream stream, bool leaveOpen = false, int bufferSize = 4096)
            : base(bufferSize, stream.CanRead, stream.CanWrite, stream.CanSeek) {
            this.BaseStream = stream;
            this._leaveOpen = leaveOpen;
        }

#if NETSTANDARD2_1_OR_GREATER
    protected override int Read(Span<byte> buffer) => this.BaseStream.Read(buffer);
    protected override void Write(ReadOnlySpan<byte> buffer) => this.BaseStream.Write(buffer);
#else
        private readonly byte[] _scratchBuffer = new byte[4096 * 4];

        protected override int Read(Span<byte> buffer) {
            int bytesRead = this.BaseStream.Read(this._scratchBuffer, 0, Math.Min(buffer.Length, this._scratchBuffer.Length));
            this._scratchBuffer.AsSpan(0, bytesRead).CopyTo(buffer);
            return bytesRead;
        }

        protected override void Write(ReadOnlySpan<byte> buffer) {
            int pos = 0;
            while (pos < buffer.Length) {
                int count = Math.Min(this._scratchBuffer.Length, buffer.Length - pos);
                buffer.Slice(pos, count).CopyTo(this._scratchBuffer);
                this.BaseStream.Write(this._scratchBuffer, 0, count);
                pos += count;
            }
        }
#endif

        protected override long Seek(long offset, SeekOrigin origin) => this.BaseStream.Seek(offset, origin);

        protected override long? GetLength() {
            try {
                return this.BaseStream.Length;
            }
            catch (NotSupportedException) {
                return null;
            }
        }

        protected override void Free() {
            base.Free();

            if (!this._leaveOpen) {
                this.BaseStream.Dispose();
            }
        }
    }
}