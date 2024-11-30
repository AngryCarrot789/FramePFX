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

using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper.Codecs;

public unsafe class VideoDecoder : MediaDecoder {
    //Used to prevent callback pointer from being GC collected
    private AVCodecContext_get_format chooseHwPixelFmt;

    public int Width => this.ctx->width;
    public int Height => this.ctx->height;
    public AVPixelFormat PixelFormat => this.ctx->pix_fmt;

    public PictureFormat FrameFormat => new PictureFormat(this.Width, this.Height, this.PixelFormat);

    public VideoDecoder(AVCodecID codecId) : this(FindCodecFromId(codecId, enc: false)) {
    }

    public VideoDecoder(AVCodec* codec) : this(AllocContext(codec)) {
    }

    public VideoDecoder(AVCodecContext* ctx, bool takeOwnership = true) : base(ctx, MediaTypes.Video, takeOwnership) {
    }

    public void SetupHardwareAccelerator(HardwareDevice device, params AVPixelFormat[] preferredPixelFormats) {
        this.ValidateNotOpen();

        this.ctx->hw_device_ctx = ffmpeg.av_buffer_ref(device.Handle);
        this.ctx->get_format = this.chooseHwPixelFmt = (ctx, pAvailFmts) => {
            for (AVPixelFormat* pFmt = pAvailFmts; *pFmt != PixelFormats.None; pFmt++) {
                if (Array.IndexOf(preferredPixelFormats, *pFmt) >= 0) {
                    return *pFmt;
                }
            }

            return PixelFormats.None;
        };
    }

    /// <summary> Returns a new list containing all hardware acceleration configurations marked with `AV_CODEC_HW_CONFIG_METHOD_HW_DEVICE_CTX`. </summary>
    public List<CodecHardwareConfig> GetHardwareConfigs() {
        this.ValidateNotDisposed();

        List<CodecHardwareConfig> configs = new List<CodecHardwareConfig>();

        int i = 0;
        AVCodecHWConfig* config;

        while ((config = ffmpeg.avcodec_get_hw_config(this.ctx->codec, i++)) != null) {
            if ((config->methods & (int) CodecHardwareMethods.DeviceContext) != 0) {
                configs.Add(new CodecHardwareConfig(this.ctx->codec, config));
            }
        }

        return configs;
    }
}