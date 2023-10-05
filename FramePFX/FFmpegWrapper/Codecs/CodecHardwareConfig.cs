using System;
using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper.Codecs {
    public unsafe readonly struct CodecHardwareConfig {
        public AVCodec* Codec { get; }
        public AVCodecHWConfig* Config { get; }

        public AVHWDeviceType DeviceType => this.Config->device_type;
        public AVPixelFormat PixelFormat => this.Config->pix_fmt;
        public CodecHardwareMethods Methods => (CodecHardwareMethods) this.Config->methods;

        public CodecHardwareConfig(AVCodec* codec, AVCodecHWConfig* config) {
            this.Codec = codec;
            this.Config = config;
        }

        public override string ToString() => new string((sbyte*) this.Codec->name) + " " + this.PixelFormat.ToString().Substring("AV_PIX_FMT_".Length);
    }

    [Flags]
    public enum CodecHardwareMethods {
        DeviceContext = 0x01,
        FramesContext = 0x02
    }
}