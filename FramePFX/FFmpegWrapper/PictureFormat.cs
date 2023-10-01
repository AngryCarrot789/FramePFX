using System;
using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper
{
    public readonly struct PictureFormat
    {
        public int Width { get; }
        public int Height { get; }
        public AVPixelFormat PixelFormat { get; }

        public int NumPlanes => ffmpeg.av_pix_fmt_count_planes(this.PixelFormat);
        public bool IsPlanar => this.NumPlanes >= 2;

        public PictureFormat(int w, int h, AVPixelFormat fmt = AVPixelFormat.AV_PIX_FMT_RGBA)
        {
            this.Width = w;
            this.Height = h;
            this.PixelFormat = fmt;
        }

        /// <param name="align">Ensures that width and height are a multiple of this value.</param>
        public PictureFormat GetScaled(int newWidth, int newHeight, AVPixelFormat newFormat = PixelFormats.None, bool keepAspectRatio = true, int align = 1)
        {
            if (keepAspectRatio)
            {
                double scale = Math.Min(newWidth / (double) this.Width, newHeight / (double) this.Height);
                newWidth = (int) Math.Round(this.Width * scale);
                newHeight = (int) Math.Round(this.Height * scale);
            }

            if (newFormat == PixelFormats.None)
            {
                newFormat = this.PixelFormat;
            }

            if (align > 1)
            {
                newWidth = (newWidth + align - 1) / align * align;
                newHeight = (newHeight + align - 1) / align * align;
            }

            return new PictureFormat(newWidth, newHeight, newFormat);
        }

        public override string ToString()
        {
            string fmt = this.PixelFormat.ToString().Substring("AV_PIX_FMT_".Length);
            return $"{this.Width}x{this.Height} {fmt}";
        }
    }
}