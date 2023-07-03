using System;
using FFmpeg.AutoGen;

namespace FramePFX.Core.FFmpegWrapper {

    public unsafe class AudioQueue : FFObject {
        private AVAudioFifo* _fifo;

        public AVAudioFifo* Handle {
            get {
                this.ValidateNotDisposed();
                return this._fifo;
            }
        }

        public AVSampleFormat Format { get; }
        public int NumChannels { get; }

        public int Size => ffmpeg.av_audio_fifo_size(this._fifo);
        public int Space => ffmpeg.av_audio_fifo_space(this._fifo);
        public int Capacity => this.Space + this.Size;

        public AudioQueue(AudioFormat fmt, int initialCapacity)
            : this(fmt.SampleFormat, fmt.NumChannels, initialCapacity) {
        }

        public AudioQueue(AVSampleFormat fmt, int numChannels, int initialCapacity) {
            this.Format = fmt;
            this.NumChannels = numChannels;

            this._fifo = ffmpeg.av_audio_fifo_alloc(fmt, numChannels, initialCapacity);
            if (this._fifo == null) {
                throw new OutOfMemoryException("Could not allocate the audio FIFO.");
            }
        }

        public void Write(AudioFrame frame) {
            if (frame.SampleFormat != this.Format || frame.NumChannels != this.NumChannels) {
                throw new ArgumentException("Incompatible frame format.", nameof(frame));
            }

            this.Write(frame.Data, frame.Count);
        }

        public void Write<T>(Span<T> src) where T : unmanaged {
            this.CheckFormatForInterleavedBuffer(src.Length, sizeof(T));

            fixed (T* pSrc = src) {
                this.Write((byte**) &pSrc, src.Length / this.NumChannels);
            }
        }

        public void Write(byte** channels, int count) {
            ffmpeg.av_audio_fifo_write(this.Handle, (void**) channels, count);
        }

        public int Read(AudioFrame frame, int count) {
            if (count <= 0 || count > frame.Capacity) {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (frame.SampleFormat != this.Format || frame.NumChannels != this.NumChannels) {
                throw new InvalidOperationException("Incompatible frame format.");
            }

            return frame.Count = this.Read(frame.Data, count);
        }

        public int Read<T>(Span<T> dest) where T : unmanaged {
            this.CheckFormatForInterleavedBuffer(dest.Length, sizeof(T));

            fixed (T* pDest = dest) {
                return this.Read((byte**) &pDest, dest.Length / this.NumChannels);
            }
        }

        public int Read(byte** dest, int count) {
            return ffmpeg.av_audio_fifo_read(this.Handle, (void**) dest, count);
        }

        public void Clear() {
            ffmpeg.av_audio_fifo_reset(this.Handle);
        }

        public void Drain(int count) {
            int tempQualifier = ffmpeg.av_audio_fifo_drain(this.Handle, count);
            if (tempQualifier < 0 && tempQualifier != ffmpeg.EAGAIN && tempQualifier != ffmpeg.AVERROR_EOF) {
                throw FFUtils.GetException(tempQualifier);
            }

            int temp = tempQualifier;
        }

        protected override void Free() {
            if (this._fifo != null) {
                ffmpeg.av_audio_fifo_free(this._fifo);
                this._fifo = null;
            }
        }

        private void ValidateNotDisposed() {
            if (this._fifo == null) {
                throw new ObjectDisposedException(nameof(AudioQueue));
            }
        }

        private void CheckFormatForInterleavedBuffer(int length, int sampleSize) {
            if (ffmpeg.av_get_bytes_per_sample(this.Format) != sampleSize ||
                ffmpeg.av_sample_fmt_is_planar(this.Format) != 0 ||
                length % this.NumChannels != 0
            ) {
                throw new InvalidOperationException("Incompatible buffer format.");
            }
        }
    }
}