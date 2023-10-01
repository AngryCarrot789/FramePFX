using System;
using System.Collections.Generic;
using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper.Codecs
{
    public unsafe class VideoDecoder : MediaDecoder
    {
        //Used to prevent callback pointer from being GC collected
        private AVCodecContext_get_format chooseHwPixelFmt;

        public int Width => this.ctx->width;
        public int Height => this.ctx->height;
        public AVPixelFormat PixelFormat => this.ctx->pix_fmt;

        public PictureFormat FrameFormat => new PictureFormat(this.Width, this.Height, this.PixelFormat);

        public VideoDecoder(AVCodecID codecId) : this(FindCodecFromId(codecId, enc: false))
        {
        }

        public VideoDecoder(AVCodec* codec) : this(AllocContext(codec))
        {
        }

        public VideoDecoder(AVCodecContext* ctx, bool takeOwnership = true) : base(ctx, MediaTypes.Video, takeOwnership)
        {
        }

        public void SetupHardwareAccelerator(HardwareDevice device, params AVPixelFormat[] preferredPixelFormats)
        {
            this.ValidateNotOpen();

            this.ctx->hw_device_ctx = ffmpeg.av_buffer_ref(device.Handle);
            this.ctx->get_format = this.chooseHwPixelFmt = (ctx, pAvailFmts) =>
            {
                for (AVPixelFormat* pFmt = pAvailFmts; *pFmt != PixelFormats.None; pFmt++)
                {
                    if (Array.IndexOf(preferredPixelFormats, *pFmt) >= 0)
                    {
                        return *pFmt;
                    }
                }

                return PixelFormats.None;
            };
        }

        /// <summary> Returns a new list containing all hardware acceleration configurations marked with `AV_CODEC_HW_CONFIG_METHOD_HW_DEVICE_CTX`. </summary>
        public List<CodecHardwareConfig> GetHardwareConfigs()
        {
            this.ValidateNotDisposed();

            List<CodecHardwareConfig> configs = new List<CodecHardwareConfig>();

            int i = 0;
            AVCodecHWConfig* config;

            while ((config = ffmpeg.avcodec_get_hw_config(this.ctx->codec, i++)) != null)
            {
                if ((config->methods & (int) CodecHardwareMethods.DeviceContext) != 0)
                {
                    configs.Add(new CodecHardwareConfig(this.ctx->codec, config));
                }
            }

            return configs;
        }
    }
}