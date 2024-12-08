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

namespace FramePFX.FFmpegWrapper;

public unsafe class HardwareDevice : FFObject
{
    private AVBufferRef* _ctx;

    public AVBufferRef* Handle
    {
        get
        {
            this.ValidateNotDisposed();
            return this._ctx;
        }
    }

    public AVHWDeviceContext* RawHandle
    {
        get
        {
            this.ValidateNotDisposed();
            return (AVHWDeviceContext*) this._ctx->data;
        }
    }

    public AVHWDeviceType Type => this.RawHandle->type;

    public HardwareDevice(AVBufferRef* deviceCtx)
    {
        this._ctx = deviceCtx;
    }

    public static HardwareDevice Create(AVHWDeviceType type)
    {
        AVBufferRef* ctx;
        if (ffmpeg.av_hwdevice_ctx_create(&ctx, type, null, null, 0) < 0)
        {
            return null;
        }

        return new HardwareDevice(ctx);
    }

    public HardwareFrameConstraints GetMaxFrameConstraints()
    {
        AVHWFramesConstraints* desc = ffmpeg.av_hwdevice_get_hwframe_constraints(this._ctx, null);
        HardwareFrameConstraints managedDesc = new HardwareFrameConstraints(desc);
        ffmpeg.av_hwframe_constraints_free(&desc);
        return managedDesc;
    }

    /// <param name="swFormat"> The pixel format identifying the actual data layout of the hardware frames. </param>
    /// <param name="initialSize"> Initial size of the frame pool. If a device type does not support dynamically resizing the pool, then this is also the maximum pool size. </param>
    public HardwareFramePool CreateFramePool(PictureFormat swFormat, int initialSize)
    {
        this.ValidateNotDisposed();

        AVBufferRef* poolRef = ffmpeg.av_hwframe_ctx_alloc(this._ctx);
        if (poolRef == null)
        {
            throw new OutOfMemoryException("Failed to allocate hardware frame pool");
        }

        AVHWFramesContext* pool = (AVHWFramesContext*) poolRef->data;
        pool->format = this.GetDefaultSurfaceFormat();
        pool->sw_format = swFormat.PixelFormat;
        pool->width = swFormat.Width;
        pool->height = swFormat.Height;
        pool->initial_pool_size = initialSize;

        if (ffmpeg.av_hwframe_ctx_init(poolRef) < 0)
        {
            ffmpeg.av_buffer_unref(&poolRef);
            return null;
        }

        return new HardwareFramePool(poolRef);
    }

    private AVPixelFormat GetDefaultSurfaceFormat()
    {
        switch (this.Type)
        {
            case HWDeviceTypes.VDPAU: return AVPixelFormat.AV_PIX_FMT_VDPAU;
            case HWDeviceTypes.Cuda: return AVPixelFormat.AV_PIX_FMT_CUDA;
            case HWDeviceTypes.VAAPI: return AVPixelFormat.AV_PIX_FMT_VAAPI;
            case HWDeviceTypes.DXVA2: return AVPixelFormat.AV_PIX_FMT_DXVA2_VLD;
            case HWDeviceTypes.QSV: return AVPixelFormat.AV_PIX_FMT_QSV;
            case HWDeviceTypes.D3D11VA: return AVPixelFormat.AV_PIX_FMT_D3D11;
            case HWDeviceTypes.DRM: return AVPixelFormat.AV_PIX_FMT_DRM_PRIME;
            case HWDeviceTypes.OpenCL: return AVPixelFormat.AV_PIX_FMT_OPENCL;
            case HWDeviceTypes.Vulkan: return AVPixelFormat.AV_PIX_FMT_VULKAN;
            case HWDeviceTypes.VideoToolbox: return AVPixelFormat.AV_PIX_FMT_VIDEOTOOLBOX;
            case HWDeviceTypes.MediaCodec: return AVPixelFormat.AV_PIX_FMT_MEDIACODEC;
            default: throw new ArgumentOutOfRangeException(nameof(this.Type), "Type is out of range");
        }
    }

    protected override void Free()
    {
        if (this._ctx != null)
        {
            fixed (AVBufferRef** ppCtx = &this._ctx)
            {
                ffmpeg.av_buffer_unref(ppCtx);
            }
        }
    }

    private void ValidateNotDisposed()
    {
        if (this._ctx == null)
        {
            throw new ObjectDisposedException(nameof(HardwareDevice));
        }
    }
}