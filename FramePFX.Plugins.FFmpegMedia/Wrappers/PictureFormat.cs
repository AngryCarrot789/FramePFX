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

namespace FramePFX.Plugins.FFmpegMedia.Wrappers;

public readonly struct PictureFormat {
    public int Width { get; }
    public int Height { get; }
    public AVPixelFormat PixelFormat { get; }

    public int NumPlanes => ffmpeg.av_pix_fmt_count_planes(this.PixelFormat);
    public bool IsPlanar => this.NumPlanes >= 2;

    public PictureFormat(int w, int h, AVPixelFormat fmt = AVPixelFormat.AV_PIX_FMT_RGBA) {
        this.Width = w;
        this.Height = h;
        this.PixelFormat = fmt;
    }

    /// <param name="align">Ensures that width and height are a multiple of this value.</param>
    public PictureFormat GetScaled(int newWidth, int newHeight, AVPixelFormat newFormat = PixelFormats.None, bool keepAspectRatio = true, int align = 1) {
        if (keepAspectRatio) {
            double scale = Math.Min(newWidth / (double) this.Width, newHeight / (double) this.Height);
            newWidth = (int) Math.Round(this.Width * scale);
            newHeight = (int) Math.Round(this.Height * scale);
        }

        if (newFormat == PixelFormats.None) {
            newFormat = this.PixelFormat;
        }

        if (align > 1) {
            newWidth = (newWidth + align - 1) / align * align;
            newHeight = (newHeight + align - 1) / align * align;
        }

        return new PictureFormat(newWidth, newHeight, newFormat);
    }

    public override string ToString() {
        string fmt = this.PixelFormat.ToString().Substring("AV_PIX_FMT_".Length);
        return $"{this.Width}x{this.Height} {fmt}";
    }
}