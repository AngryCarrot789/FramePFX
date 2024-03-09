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
using System.Collections.Generic;
using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper.Codecs
{
    public unsafe class VideoEncoder : MediaEncoder
    {
        public int Width {
            get => this.ctx->width;
            set => this.SetOrThrowIfOpen(ref this.ctx->width, value);
        }

        public int Height {
            get => this.ctx->height;
            set => this.SetOrThrowIfOpen(ref this.ctx->height, value);
        }

        public AVPixelFormat PixelFormat {
            get => this.ctx->pix_fmt;
            set => this.SetOrThrowIfOpen(ref this.ctx->pix_fmt, value);
        }

        public PictureFormat FrameFormat {
            get => new PictureFormat(this.Width, this.Height, this.PixelFormat);
            set
            {
                this.ctx->width = value.Width;
                this.ctx->height = value.Height;
                this.ctx->pix_fmt = value.PixelFormat;
            }
        }

        public int GopSize {
            get => this.ctx->gop_size;
            set => this.SetOrThrowIfOpen(ref this.ctx->gop_size, value);
        }

        public int MaxBFrames {
            get => this.ctx->max_b_frames;
            set => this.SetOrThrowIfOpen(ref this.ctx->max_b_frames, value);
        }

        public int MinQuantizer {
            get => this.ctx->qmin;
            set => this.SetOrThrowIfOpen(ref this.ctx->qmin, value);
        }

        public int MaxQuantizer {
            get => this.ctx->qmax;
            set => this.SetOrThrowIfOpen(ref this.ctx->qmax, value);
        }

        public int CompressionLevel {
            get => this.ctx->compression_level;
            set => this.SetOrThrowIfOpen(ref this.ctx->compression_level, value);
        }

        public ReadOnlySpan<AVPixelFormat> SupportedPixelFormats
            => FFUtils.GetSpanFromSentinelTerminatedPtr(this.ctx->codec->pix_fmts, PixelFormats.None);

        public VideoEncoder(AVCodecID codecId, in PictureFormat format, double frameRate, int bitrate) : this(FindCodecFromId(codecId, enc: true), format, frameRate, bitrate)
        {
        }

        public VideoEncoder(AVCodec* codec, in PictureFormat format, double frameRate, int bitrate) : this(AllocContext(codec))
        {
            this.FrameFormat = format;
            this.FrameRate = ffmpeg.av_d2q(frameRate, 100_000);
            this.TimeBase = ffmpeg.av_inv_q(this.FrameRate);
            this.BitRate = bitrate;
        }

        public VideoEncoder(AVCodecContext* ctx, bool takeOwnership = true) : base(ctx, MediaTypes.Video, takeOwnership)
        {
        }

        public VideoEncoder(CodecHardwareConfig config, in PictureFormat format, double frameRate, int bitrate, HardwareDevice device, HardwareFramePool framePool) : this(config.Codec, in format, frameRate, bitrate)
        {
            this.ctx->hw_device_ctx = ffmpeg.av_buffer_ref(device.Handle);
            this.ctx->hw_frames_ctx = framePool == null ? null : ffmpeg.av_buffer_ref(framePool.Handle);

            if (framePool == null && (config.Methods & ~CodecHardwareMethods.FramesContext) == 0)
            {
                throw new ArgumentException("Specified hardware encoder config requires a frame pool to be provided.");
            }
        }

        /// <summary> Returns the correct <see cref="MediaFrame.PresentationTimestamp"/> for the given frame number, in respect to <see cref="CodecBase.FrameRate"/> and <see cref="CodecBase.TimeBase"/>. </summary>
        public long GetFramePts(long frameNumber)
        {
            return ffmpeg.av_rescale_q(frameNumber, ffmpeg.av_inv_q(this.FrameRate), this.TimeBase);
        }

        public static HardwareDevice CreateCompatibleHardwareDevice(AVCodecID codecId, in PictureFormat format, out CodecHardwareConfig codecConfig)
        {
            foreach (CodecHardwareConfig config in GetHardwareConfigs(codecId))
            {
                if (config.PixelFormat != format.PixelFormat)
                    continue;

                HardwareDevice device = HardwareDevice.Create(config.DeviceType);

                if (device != null && device.GetMaxFrameConstraints().IsValidFormat(format))
                {
                    codecConfig = config;
                    return device;
                }

                device?.Dispose();
            }

            codecConfig = default;
            return null;
        }

        public static List<CodecHardwareConfig> GetHardwareConfigs(AVCodecID? codecId = null, AVHWDeviceType? deviceType = null)
        {
            List<CodecHardwareConfig> configs = new List<CodecHardwareConfig>();

            void* iterState = null;
            AVCodec* codec;

            while ((codec = ffmpeg.av_codec_iterate(&iterState)) != null)
            {
                if ((codecId != null && codec->id != codecId) || ffmpeg.av_codec_is_encoder(codec) == 0)
                    continue;

                int i = 0;
                AVCodecHWConfig* config;

                while ((config = ffmpeg.avcodec_get_hw_config(codec, i++)) != null)
                {
                    const int reqMethods = (int) (CodecHardwareMethods.DeviceContext | CodecHardwareMethods.FramesContext);

                    if ((config->methods & reqMethods) != 0 && (deviceType == null || config->device_type == deviceType))
                    {
                        configs.Add(new CodecHardwareConfig(codec, config));
                    }
                }
            }

            return configs;
        }
    }
}