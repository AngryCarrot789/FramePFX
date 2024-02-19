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
using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper {
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

        public AudioQueue(AudioFormat fmt, int initialCapacity) : this(fmt.SampleFormat, fmt.NumChannels, initialCapacity) {
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