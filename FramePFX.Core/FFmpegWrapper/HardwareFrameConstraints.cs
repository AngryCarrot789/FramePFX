using System;
using FFmpeg.AutoGen;

namespace FramePFX.Core.FFmpegWrapper {
    public readonly struct HardwareFrameConstraints {
        public AVPixelFormat[] ValidHardwareFormats { get; }
        public AVPixelFormat[] ValidSoftwareFormats { get; }

        public int MinWidth { get; }
        public int MinHeight { get; }

        public int MaxWidth { get; }
        public int MaxHeight { get; }

        public unsafe HardwareFrameConstraints(AVHWFramesConstraints* desc) {
            this.ValidHardwareFormats = FFUtils.GetSpanFromSentinelTerminatedPtr(desc->valid_hw_formats, PixelFormats.None).ToArray();
            this.ValidSoftwareFormats = FFUtils.GetSpanFromSentinelTerminatedPtr(desc->valid_sw_formats, PixelFormats.None).ToArray();
            this.MinWidth = desc->min_width;
            this.MinHeight = desc->min_height;
            this.MaxWidth = desc->max_width;
            this.MaxHeight = desc->max_height;
        }

        public bool IsValidDimensions(int width, int height) {
            return width >= this.MinWidth && width <= this.MaxWidth &&
                   height >= this.MinHeight && height <= this.MaxHeight;
        }

        public bool IsValidFormat(in PictureFormat format) {
            return this.IsValidDimensions(format.Width, format.Height) &&
                   Array.IndexOf(this.ValidSoftwareFormats, format.PixelFormat) >= 0;
        }
    }
}