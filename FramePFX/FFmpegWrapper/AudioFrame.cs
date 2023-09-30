using System;
using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper {
    public unsafe class AudioFrame : MediaFrame {
        public AVSampleFormat SampleFormat => (AVSampleFormat) this.frame->format;
        public int SampleRate => this.frame->sample_rate;
        public int NumChannels => this.frame->ch_layout.nb_channels;

        public AudioFormat Format => new AudioFormat(this.frame);

        public byte** Data => (byte**) &this.frame->data;
        public int Stride => this.frame->linesize[0];

        public bool IsPlanar => ffmpeg.av_sample_fmt_is_planar(this.SampleFormat) != 0;

        public int Count {
            get => this.frame->nb_samples;
            set {
                if (value < 0 || value > this.Capacity) {
                    throw new ArgumentOutOfRangeException(nameof(value), "Must must be positive and not exceed the frame capacity.");
                }

                this.frame->nb_samples = value;
            }
        }

        public int Capacity => this.Stride / (ffmpeg.av_get_bytes_per_sample(this.SampleFormat) * (this.IsPlanar ? 1 : this.NumChannels));

        /// <summary> Allocates a new empty <see cref="AVFrame"/>. </summary>
        public AudioFrame() : this(ffmpeg.av_frame_alloc(), takeOwnership: true) {
        }

        public AudioFrame(AudioFormat fmt, int capacity) {
            this.frame = ffmpeg.av_frame_alloc();
            this.frame->format = (int) fmt.SampleFormat;
            this.frame->sample_rate = fmt.SampleRate;
            this.frame->ch_layout = fmt.Layout;

            this.frame->nb_samples = capacity;
            FFUtils.CheckError(ffmpeg.av_frame_get_buffer(this.frame, 0), "Failed to allocate frame buffers.");
        }

        public AudioFrame(AVSampleFormat fmt, int sampleRate, int numChannels, int capacity) : this(new AudioFormat(fmt, sampleRate, numChannels), capacity) {
        }

        /// <summary> Wraps an existing <see cref="AVFrame"/> into an <see cref="AudioFrame"/> instance. </summary>
        /// <param name="takeOwnership">True if <paramref name="frame"/> should be freed when Dispose() is called.</param>
        public AudioFrame(AVFrame* frame, bool takeOwnership = false) {
            if (frame == null) {
                throw new ArgumentNullException(nameof(frame));
            }

            this.frame = frame;
            this._ownsFrame = takeOwnership;
        }

        public Span<T> GetChannelSamples<T>(int channel = 0) where T : unmanaged {
            if ((uint) channel >= (uint) this.NumChannels || (!this.IsPlanar && channel != 0)) {
                throw new ArgumentOutOfRangeException();
            }

            return new Span<T>(this.Data[channel], this.Stride / sizeof(T));
        }

        /// <summary> Copy interleaved samples from the span into this frame. </summary>
        /// <returns> Returns the number of samples copied. </returns>
        public int CopyFrom(Span<float> samples) => this.CopyFrom<float>(samples);

        /// <inheritdoc cref="CopyFrom(Span{float})"/>
        public int CopyFrom(Span<short> samples) => this.CopyFrom<short>(samples);

        private int CopyFrom<T>(Span<T> samples) where T : unmanaged {
            AudioFormat fmt = this.Format;
            if (fmt.IsPlanar || fmt.BytesPerSample != sizeof(T)) {
                throw new InvalidOperationException("Incompatible format");
            }

            if (samples.Length % fmt.NumChannels != 0) {
                throw new ArgumentException("Sample count must be a multiple of channel count.", nameof(samples));
            }

            int count = Math.Min(this.Capacity, samples.Length / fmt.NumChannels);

            fixed (T* ptr = samples) {
                byte** temp = stackalloc byte*[1] {(byte*) ptr};
                ffmpeg.av_samples_copy(this.frame->extended_data, temp, 0, 0, count, fmt.NumChannels, fmt.SampleFormat);
            }

            return count;
        }
    }
}