using System;
using FFmpeg.AutoGen;

namespace FramePFX.Core.FFmpegWrapper.Codecs
{
    public abstract unsafe class CodecBase : FFObject
    {
        protected AVCodecContext* ctx;
        protected readonly bool contextOwner;
        private bool hasUserExtraData;

        public AVCodecContext* Handle
        {
            get
            {
                this.ValidateNotDisposed();
                return this.ctx;
            }
        }

        public bool IsOpen => ffmpeg.avcodec_is_open(this.Handle) != 0;

        public string CodecName => new string((sbyte*) this.ctx->codec->name);
        public string CodecLongName => new string((sbyte*) this.ctx->codec->long_name);

        public AVRational TimeBase
        {
            get => this.ctx->time_base;
            set => this.SetOrThrowIfOpen(ref this.ctx->time_base, value);
        }

        public AVRational FrameRate
        {
            get => this.ctx->framerate;
            set => this.SetOrThrowIfOpen(ref this.ctx->framerate, value);
        }

        public Span<byte> ExtraData
        {
            get => this.GetExtraData();
            set => this.SetExtraData(value);
        }

        /// <summary> Indicates if the codec requires flushing with NULL input at the end in order to give the complete and correct output. </summary>
        public bool IsDelayed => (this.ctx->codec->capabilities & ffmpeg.AV_CODEC_CAP_DELAY) != 0;

        public AVMediaType CodecType => this.ctx->codec_type;

        internal CodecBase(AVCodecContext* ctx, AVMediaType expectedType, bool takeOwnership = true)
        {
            if (ctx->codec->type != expectedType)
            {
                if (takeOwnership)
                    ffmpeg.avcodec_free_context(&ctx);

                throw new ArgumentException("Specified codec is not valid for the current media type.");
            }

            this.ctx = ctx;
            this.contextOwner = false;
        }

        protected static AVCodec* FindCodecFromId(AVCodecID codecId, bool enc)
        {
            AVCodec* codec = enc
                ? ffmpeg.avcodec_find_encoder(codecId)
                : ffmpeg.avcodec_find_decoder(codecId);

            if (codec == null)
            {
                throw new NotSupportedException($"Could not find {(enc ? "decoder" : "encoder")} for codec {codecId.ToString().Substring("AV_CODEC_ID_".Length)}.");
            }

            return codec;
        }

        protected static AVCodecContext* AllocContext(AVCodec* codec)
        {
            AVCodecContext* ctx = ffmpeg.avcodec_alloc_context3(codec);
            if (ctx == null)
            {
                throw new OutOfMemoryException("Failed to allocate codec context.");
            }

            return ctx;
        }

        public void Open()
        {
            if (!this.IsOpen)
            {
                FFUtils.CheckError(ffmpeg.avcodec_open2(this.Handle, null, null), "Could not open codec");
            }
        }

        /// <summary> Initializes the codec if not already. </summary>
        public void Open(AVDictionary* options)
        {
            if (!this.IsOpen)
            {
                FFUtils.CheckError(ffmpeg.avcodec_open2(this.Handle, null, &options), "Could not open codec");
            }
        }

        /// <summary> Enables or disables multi-threading if supported by the codec implementation. </summary>
        /// <param name="threadCount">Number of threads to use. 1 to disable multi-threading, 0 to automatically pick a value.</param>
        /// <param name="preferFrameSlices">Allow only multi-threaded processing of frame slices rather than individual frames. Setting to true may reduce delay. </param>
        public void SetThreadCount(int threadCount, bool preferFrameSlices = false)
        {
            this.ValidateNotOpen();
            this.ctx->thread_count = threadCount;
            int caps = this.ctx->codec->capabilities;

            if ((caps & ffmpeg.AV_CODEC_CAP_SLICE_THREADS) != 0 && preferFrameSlices)
            {
                this.ctx->thread_type = ffmpeg.FF_THREAD_SLICE;
                return;
            }

            if ((caps & ffmpeg.AV_CODEC_CAP_FRAME_THREADS) != 0)
            {
                this.ctx->thread_type = ffmpeg.FF_THREAD_FRAME;
                return;
            }

            this.ctx->thread_count = 1; //no multi-threading capability
        }

        /// <summary> Reset the decoder state / flush internal buffers. </summary>
        public virtual void Flush()
        {
            if (!this.IsOpen)
            {
                throw new InvalidOperationException("Cannot flush closed codec");
            }

            ffmpeg.avcodec_flush_buffers(this.Handle);
        }

        private Span<byte> GetExtraData()
        {
            return new Span<byte>(this.ctx->extradata, this.ctx->extradata_size);
        }

        private void SetExtraData(Span<byte> buf)
        {
            this.ValidateNotOpen();

            ffmpeg.av_freep(&this.ctx->extradata);

            if (buf.IsEmpty)
            {
                this.ctx->extradata = null;
                this.ctx->extradata_size = 0;
            }
            else
            {
                this.ctx->extradata = (byte*) ffmpeg.av_mallocz((ulong) buf.Length + ffmpeg.AV_INPUT_BUFFER_PADDING_SIZE);
                this.ctx->extradata_size = buf.Length;
                buf.CopyTo(new Span<byte>(this.ctx->extradata, buf.Length));
                this.hasUserExtraData = true;
            }
        }

        protected void SetOrThrowIfOpen<T>(ref T loc, T value)
        {
            this.ValidateNotOpen();
            loc = value;
        }

        protected void ValidateNotOpen()
        {
            this.ValidateNotDisposed();

            if (this.IsOpen)
            {
                throw new InvalidOperationException("Value must be set before the codec is open.");
            }
        }

        protected override void Free()
        {
            if (this.ctx != null)
            {
                if (this.hasUserExtraData)
                {
                    ffmpeg.av_freep(&this.ctx->extradata);
                }

                if (this.contextOwner)
                {
                    fixed (AVCodecContext** c = &this.ctx)
                    {
                        ffmpeg.avcodec_free_context(c);
                    }
                }
                else
                {
                    this.ctx = null;
                }
            }
        }

        protected void ValidateNotDisposed()
        {
            if (this.ctx == null)
            {
                throw new ObjectDisposedException(nameof(CodecBase));
            }
        }
    }
}