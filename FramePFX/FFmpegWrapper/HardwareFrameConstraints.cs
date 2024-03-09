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

namespace FramePFX.FFmpegWrapper
{
    public readonly struct HardwareFrameConstraints
    {
        public AVPixelFormat[] ValidHardwareFormats { get; }
        public AVPixelFormat[] ValidSoftwareFormats { get; }

        public int MinWidth { get; }
        public int MinHeight { get; }

        public int MaxWidth { get; }
        public int MaxHeight { get; }

        public unsafe HardwareFrameConstraints(AVHWFramesConstraints* desc)
        {
            this.ValidHardwareFormats = FFUtils.GetSpanFromSentinelTerminatedPtr(desc->valid_hw_formats, PixelFormats.None).ToArray();
            this.ValidSoftwareFormats = FFUtils.GetSpanFromSentinelTerminatedPtr(desc->valid_sw_formats, PixelFormats.None).ToArray();
            this.MinWidth = desc->min_width;
            this.MinHeight = desc->min_height;
            this.MaxWidth = desc->max_width;
            this.MaxHeight = desc->max_height;
        }

        public bool IsValidDimensions(int width, int height)
        {
            return width >= this.MinWidth && width <= this.MaxWidth &&
                   height >= this.MinHeight && height <= this.MaxHeight;
        }

        public bool IsValidFormat(in PictureFormat format)
        {
            return this.IsValidDimensions(format.Width, format.Height) &&
                   Array.IndexOf(this.ValidSoftwareFormats, format.PixelFormat) >= 0;
        }
    }
}