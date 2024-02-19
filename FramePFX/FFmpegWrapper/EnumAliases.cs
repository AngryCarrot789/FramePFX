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

namespace FramePFX.FFmpegWrapper {
    public static class PixelFormats {
        public const AVPixelFormat None = AVPixelFormat.AV_PIX_FMT_NONE;

        /// <summary>planar YUV 4:2:0, 12bpp, (1 Cr &amp; Cb sample per 2x2 Y samples)</summary>
        public const AVPixelFormat YUV420P = AVPixelFormat.AV_PIX_FMT_YUV420P;

        /// <summary>planar YUV 4:2:2, 16bpp, (1 Cr &amp; Cb sample per 2x1 Y samples)</summary>
        public const AVPixelFormat YUV422P = AVPixelFormat.AV_PIX_FMT_YUV422P;

        /// <summary>planar YUV 4:4:4, 24bpp, (1 Cr &amp; Cb sample per 1x1 Y samples)</summary>
        public const AVPixelFormat YUV444P = AVPixelFormat.AV_PIX_FMT_YUV444P;

        /// <summary>planar YUV 4:1:0, 9bpp, (1 Cr &amp; Cb sample per 4x4 Y samples)</summary>
        public const AVPixelFormat YUV410P = AVPixelFormat.AV_PIX_FMT_YUV410P;

        /// <summary>planar YUV 4:1:1, 12bpp, (1 Cr &amp; Cb sample per 4x1 Y samples)</summary>
        public const AVPixelFormat YUV411P = AVPixelFormat.AV_PIX_FMT_YUV411P;

        /// <summary>planar YUV 4:2:0, 12bpp, 1 plane for Y and 1 plane for the UV components, which are interleaved (first byte U and the following byte V)</summary>
        public const AVPixelFormat NV12 = AVPixelFormat.AV_PIX_FMT_NV12;

        /// <summary>like NV12, with 10bpp per component, data in the high bits, zeros in the low bits, little-endian (each component takes 16-bits)</summary>
        public const AVPixelFormat P010LE = AVPixelFormat.AV_PIX_FMT_P010LE;

        /// <summary>like NV12, with 10bpp per component, data in the high bits, zeros in the low bits, big-endian (each component takes 16-bits)</summary>
        public const AVPixelFormat P010BE = AVPixelFormat.AV_PIX_FMT_P010BE;

        /// <summary>packed RGB 8:8:8, 24bpp, RGBRGB...</summary>
        public const AVPixelFormat RGB24 = AVPixelFormat.AV_PIX_FMT_RGB24;

        /// <summary>packed RGB 8:8:8, 24bpp, BGRBGR...</summary>
        public const AVPixelFormat BGR24 = AVPixelFormat.AV_PIX_FMT_BGR24;

        /// <summary>packed ARGB 8:8:8:8, 32bpp, ARGBARGB...</summary>
        public const AVPixelFormat ARGB = AVPixelFormat.AV_PIX_FMT_ARGB;

        /// <summary>packed RGBA 8:8:8:8, 32bpp, RGBARGBA...</summary>
        public const AVPixelFormat RGBA = AVPixelFormat.AV_PIX_FMT_RGBA;

        /// <summary>packed ABGR 8:8:8:8, 32bpp, ABGRABGR...</summary>
        public const AVPixelFormat ABGR = AVPixelFormat.AV_PIX_FMT_ABGR;

        /// <summary>packed BGRA 8:8:8:8, 32bpp, BGRABGRA...</summary>
        public const AVPixelFormat BGRA = AVPixelFormat.AV_PIX_FMT_BGRA;

        /// <summary>packed RGB 8:8:8, 32bpp, XRGBXRGB... X=unused/undefined</summary>
        public const AVPixelFormat XRGB = AVPixelFormat.AV_PIX_FMT_0RGB;

        /// <summary>packed RGB 8:8:8, 32bpp, RGBXRGBX... X=unused/undefined</summary>
        public const AVPixelFormat RGBX = AVPixelFormat.AV_PIX_FMT_RGB0;

        /// <summary>packed BGR 8:8:8, 32bpp, XBGRXBGR... X=unused/undefined</summary>
        public const AVPixelFormat XBGR = AVPixelFormat.AV_PIX_FMT_0BGR;

        /// <summary>packed BGR 8:8:8, 32bpp, BGRXBGRX... X=unused/undefined</summary>
        public const AVPixelFormat BGRX = AVPixelFormat.AV_PIX_FMT_BGR0;

        /// <summary>Y, 8bpp</summary>
        public const AVPixelFormat Gray8 = AVPixelFormat.AV_PIX_FMT_GRAY8;
    }

    public static class SampleFormats {
        /// <summary> signed 16 bits </summary>
        public const AVSampleFormat S16 = AVSampleFormat.AV_SAMPLE_FMT_S16;

        /// <summary> signed 16 bits, planar </summary>
        public const AVSampleFormat S16Planar = AVSampleFormat.AV_SAMPLE_FMT_S16P;

        /// <summary> 32 bits floating point </summary>
        public const AVSampleFormat Float = AVSampleFormat.AV_SAMPLE_FMT_FLT;

        /// <summary> 32 bits floating point, planar </summary>
        public const AVSampleFormat FloatPlanar = AVSampleFormat.AV_SAMPLE_FMT_FLTP;
    }

    public static class CodecIds {
        public const AVCodecID H264 = AVCodecID.AV_CODEC_ID_H264;
        public const AVCodecID HEVC = AVCodecID.AV_CODEC_ID_HEVC;

        public const AVCodecID VP8 = AVCodecID.AV_CODEC_ID_VP8;
        public const AVCodecID VP9 = AVCodecID.AV_CODEC_ID_VP9;
        public const AVCodecID AV1 = AVCodecID.AV_CODEC_ID_AV1;

        public const AVCodecID MP3 = AVCodecID.AV_CODEC_ID_MP3;
        public const AVCodecID AAC = AVCodecID.AV_CODEC_ID_AAC;
        public const AVCodecID AC3 = AVCodecID.AV_CODEC_ID_AC3;

        public const AVCodecID FLAC = AVCodecID.AV_CODEC_ID_FLAC;

        public const AVCodecID Vorbis = AVCodecID.AV_CODEC_ID_VORBIS;
        public const AVCodecID Opus = AVCodecID.AV_CODEC_ID_OPUS;
    }

    public static class MediaTypes {
        public const AVMediaType
            Unknown = AVMediaType.AVMEDIA_TYPE_UNKNOWN,
            Video = AVMediaType.AVMEDIA_TYPE_VIDEO,
            Audio = AVMediaType.AVMEDIA_TYPE_AUDIO,
            Subtitle = AVMediaType.AVMEDIA_TYPE_SUBTITLE;
    }

    public static class HWDeviceTypes {
        public const AVHWDeviceType
            None = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE,
            VDPAU = AVHWDeviceType.AV_HWDEVICE_TYPE_VDPAU,
            Cuda = AVHWDeviceType.AV_HWDEVICE_TYPE_CUDA,
            VAAPI = AVHWDeviceType.AV_HWDEVICE_TYPE_VAAPI,
            DXVA2 = AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2,
            QSV = AVHWDeviceType.AV_HWDEVICE_TYPE_QSV,
            D3D11VA = AVHWDeviceType.AV_HWDEVICE_TYPE_D3D11VA,
            DRM = AVHWDeviceType.AV_HWDEVICE_TYPE_DRM,
            OpenCL = AVHWDeviceType.AV_HWDEVICE_TYPE_OPENCL,
            Vulkan = AVHWDeviceType.AV_HWDEVICE_TYPE_VULKAN,
            VideoToolbox = AVHWDeviceType.AV_HWDEVICE_TYPE_VIDEOTOOLBOX,
            MediaCodec = AVHWDeviceType.AV_HWDEVICE_TYPE_MEDIACODEC;
    }
}