using System;
using FFmpeg.AutoGen;

namespace FramePFX.FFmpegWrapper
{
    public unsafe class SwScaler : FFObject
    {
        private SwsContext* _ctx;

        public SwsContext* Handle
        {
            get
            {
                this.ValidateNotDisposed();
                return this._ctx;
            }
        }

        public PictureFormat InputFormat { get; }
        public PictureFormat OutputFormat { get; }

        public SwScaler(PictureFormat inFmt, PictureFormat outFmt, InterpolationMode flags = InterpolationMode.Bicubic)
        {
            this.InputFormat = inFmt;
            this.OutputFormat = outFmt;

            this._ctx = ffmpeg.sws_getContext(inFmt.Width, inFmt.Height, inFmt.PixelFormat,
                outFmt.Width, outFmt.Height, outFmt.PixelFormat,
                (int) flags, null, null, null);
        }

        public void Convert(VideoFrame src, VideoFrame dst)
        {
            this.Convert(src.Handle, dst.Handle);
        }

        public void Convert(AVFrame* src, AVFrame* dst)
        {
            PictureFormat srcFmt = this.InputFormat;
            PictureFormat dstFmt = this.OutputFormat;

            if ((src->format != (int) srcFmt.PixelFormat || src->width != srcFmt.Width || src->height != srcFmt.Height) ||
                (dst->format != (int) dstFmt.PixelFormat || dst->width != dstFmt.Width || dst->height != dstFmt.Height)
            )
            {
                throw new ArgumentException("Frame must match rescaler formats");
            }

            ffmpeg.sws_scale(this.Handle, src->data, src->linesize, 0, this.InputFormat.Height, dst->data, dst->linesize);
        }

        /// <summary> Converts and rescale interleaved pixel data into the destination format.</summary>
        public void Convert(ReadOnlySpan<byte> src, int stride, VideoFrame dst)
        {
            PictureFormat srcFmt = this.InputFormat;
            PictureFormat dstFmt = this.OutputFormat;

            if ((srcFmt.IsPlanar || src.Length < srcFmt.Height * stride) ||
                (dst.PixelFormat != dstFmt.PixelFormat || dst.Width != dstFmt.Width || dst.Height != dstFmt.Height)
            )
            {
                throw new ArgumentException("Frame must match rescaler formats");
            }

            fixed (byte* pSrc = src)
            {
                ffmpeg.sws_scale(this.Handle, new[] {pSrc}, new[] {stride}, 0, dst.Height, dst.Handle->data, dst.Handle->linesize);
            }
        }

        protected override void Free()
        {
            if (this._ctx != null)
            {
                ffmpeg.sws_freeContext(this._ctx);
                this._ctx = null;
            }
        }

        private void ValidateNotDisposed()
        {
            if (this._ctx == null)
            {
                throw new ObjectDisposedException(nameof(SwScaler));
            }
        }
    }

    public enum InterpolationMode
    {
        FastBilinear = ffmpeg.SWS_FAST_BILINEAR,
        Bilinear = ffmpeg.SWS_BILINEAR,
        Bicubic = ffmpeg.SWS_BICUBIC,
        NearestNeighbor = ffmpeg.SWS_POINT,
        Box = ffmpeg.SWS_AREA,
        Gaussian = ffmpeg.SWS_GAUSS,
        Sinc = ffmpeg.SWS_SINC,
        Lanczos = ffmpeg.SWS_LANCZOS,
        Spline = ffmpeg.SWS_SPLINE
    }
}