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
using FramePFX.FFmpegWrapper.Codecs;

namespace FramePFX.FFmpegWrapper;

public unsafe class VideoFrame : MediaFrame {
    public int Width => this.frame->width;
    public int Height => this.frame->height;
    public AVPixelFormat PixelFormat => (AVPixelFormat) this.frame->format;

    public PictureFormat Format => new PictureFormat(this.Width, this.Height, this.PixelFormat);

    /// <summary> Pointers to the pixel data planes. </summary>
    /// <remarks> These can point to the end of image data when used in combination with negative values in <see cref="RowSize"/>. </remarks>
    public byte** Data => (byte**) &this.frame->data;

    /// <summary> An array of positive or negative values indicating the size in bytes of each pixel row. </summary>
    /// <remarks>
    /// - Values may be larger than the size of usable data -- there may be extra padding present for performance reasons. <br/>
    /// - Values can be negative to achieve a vertically inverted iteration over image rows.
    /// </remarks>
    public int* RowSize => (int*) &this.frame->linesize;

    /// <summary> Whether this frame is attached to a hardware frame context. </summary>
    public bool IsHardwareFrame => this.frame->hw_frames_ctx != null;

    /// <summary> Whether the frame rows are flipped. Alias for <c>RowSize[0] &lt; 0</c>. </summary>
    public bool IsVerticallyFlipped => this.frame->linesize[0] < 0;

    /// <summary> Allocates a new empty <see cref="AVFrame"/>. </summary>
    public VideoFrame() : this(ffmpeg.av_frame_alloc(), clearToBlack: false, takeOwnership: true) {
    }

    public VideoFrame(PictureFormat fmt, bool clearToBlack = true) : this(fmt.Width, fmt.Height, fmt.PixelFormat, clearToBlack) {
    }

    public VideoFrame(int width, int height, AVPixelFormat fmt = AVPixelFormat.AV_PIX_FMT_RGBA, bool clearToBlack = true) {
        if (width <= 0 || height <= 0) {
            throw new ArgumentException("Invalid frame dimensions.");
        }

        this.frame = ffmpeg.av_frame_alloc();
        this.frame->format = (int) fmt;
        this.frame->width = width;
        this.frame->height = height;

        FFUtils.CheckError(ffmpeg.av_frame_get_buffer(this.frame, 0), "Failed to allocate frame buffers.");

        if (clearToBlack) {
            this.Clear();
        }
    }

    /// <summary> Wraps an existing <see cref="AVFrame"/> pointer. </summary>
    /// <param name="takeOwnership">True if <paramref name="frame"/> should be freed when Dispose() is called.</param>
    public VideoFrame(AVFrame* frame, bool clearToBlack = false, bool takeOwnership = false) {
        if (frame == null) {
            throw new ArgumentNullException(nameof(frame));
        }

        this.frame = frame;
        this._ownsFrame = takeOwnership;

        if (clearToBlack) {
            this.Clear();
        }
    }

    /// <summary> Returns a view over the pixel row for the specified plane. </summary>
    /// <remarks> The returned span may be longer than <see cref="Width"/> due to padding. </remarks>
    /// <param name="y">Row index, in top to bottom order.</param>
    public Span<T> GetRowSpan<T>(int y, int plane = 0) where T : unmanaged {
        if ((uint) y >= (uint) this.GetPlaneSize(plane).Height) {
            throw new ArgumentOutOfRangeException();
        }

        int stride = this.RowSize[plane];
        return new Span<T>(&this.Data[plane][y * stride], Math.Abs(stride / sizeof(T)));
    }

    /// <summary> Returns a view over the pixel data for the specified plane. </summary>
    /// <remarks> Note that rows may be stored in reverse order depending on <see cref="IsVerticallyFlipped"/>. </remarks>
    /// <param name="stride">Number of pixels per row.</param>
    public Span<T> GetPlaneSpan<T>(int plane, out int stride) where T : unmanaged {
        int height = this.GetPlaneSize(plane).Height;

        byte* data = this.frame->data[(uint) plane];
        int rowSize = this.frame->linesize[(uint) plane];

        if (rowSize < 0) {
            data += rowSize * (height - 1);
            // possible underflow in unchecked context?
            rowSize *= -1;
        }

        stride = rowSize / sizeof(T);
        return new Span<T>(data, checked(height * stride));
    }

    public (int Width, int Height) GetPlaneSize(int plane) {
        this.ValidateNotDisposed();

        (int Width, int Height) size = (this.Width, this.Height);

        //https://github.com/FFmpeg/FFmpeg/blob/c558fcf41e2027a1096d00b286954da2cc4ae73f/libavutil/imgutils.c#L111
        if (plane == 0) {
            return size;
        }

        AVPixFmtDescriptor* desc = ffmpeg.av_pix_fmt_desc_get(this.PixelFormat);

        if (desc == null || (desc->flags & ffmpeg.AV_PIX_FMT_FLAG_HWACCEL) != 0) {
            throw new InvalidOperationException();
        }

        for (uint i = 0; i < 4; i++) {
            if (desc->comp[i].plane != plane)
                continue;

            if ((i == 1 || i == 2) && (desc->flags & ffmpeg.AV_PIX_FMT_FLAG_RGB) == 0) {
                size.Width = CeilShr(size.Width, desc->log2_chroma_w);
                size.Height = CeilShr(size.Height, desc->log2_chroma_h);
            }

            return size;
        }

        throw new ArgumentOutOfRangeException(nameof(plane));
    }

    internal static int CeilShr(int x, int s) => (x + (1 << s) - 1) >> s;

    /// <summary> Creates a hardware frame memory mapping. </summary>
    public VideoFrame Map(HardwareFrameMappingFlags flags) {
        this.ValidateNotDisposed();

        AVFrame* mapping = ffmpeg.av_frame_alloc();
        int result = ffmpeg.av_hwframe_map(mapping, this.frame, (int) flags);

        if (result == 0) {
            mapping->width = this.frame->width;
            mapping->height = this.frame->height;
            return new VideoFrame(mapping, takeOwnership: true);
        }

        ffmpeg.av_frame_free(&mapping);
        throw FFUtils.GetException(result, "Failed to create hardware frame mapping");
    }

    /// <summary> Copy data from this frame to <paramref name="dest"/>. At least one of <see langword="this"/> or <paramref name="dest"/> must be a hardware frame. </summary>
    public void TransferTo(VideoFrame dest) {
        this.ValidateNotDisposed();
        if (this.frame == null)
            throw new ObjectDisposedException("this", "This object is disposed due to a null frame");
        
        FFUtils.CheckError(ffmpeg.av_hwframe_transfer_data(dest.Handle, this.frame, 0), "Failed to transfer data from hardware frame");
    }

    /// <summary> Gets an array of possible source or dest formats usable in <see cref="TransferTo(VideoFrame)"/>. </summary>
    public AVPixelFormat[] GetHardwareTransferFormats(HardwareFrameTransferDirection direction) {
        this.ValidateNotDisposed();
        AVPixelFormat* pFormats;

        if (ffmpeg.av_hwframe_transfer_get_formats(this.frame->hw_frames_ctx, (AVHWFrameTransferDirection) direction, &pFormats, 0) < 0) {
            return Array.Empty<AVPixelFormat>();
        }

        AVPixelFormat[] formats = FFUtils.GetSpanFromSentinelTerminatedPtr(pFormats, PixelFormats.None).ToArray();
        ffmpeg.av_freep(&pFormats);

        return formats;
    }

    /// <summary> Fills this frame with black pixels. </summary>
    public void Clear() {
        this.ValidateNotDisposed();
        long_array4 linesizes = new long_array4();

        for (uint i = 0; i < 4; i++) {
            linesizes[i] = this.frame->linesize[i];
        }

        FFUtils.CheckError(ffmpeg.av_image_fill_black(
            ref *(byte_ptrArray4*) &this.frame->data, linesizes, this.PixelFormat, this.frame->color_range, this.frame->width, this.frame->height
        ), "Failed to clear frame.");
    }

    /// <summary> Saves this frame to the specified file. The format will be choosen based on the file extension. (Can be either JPG or PNG) </summary>
    /// <remarks> This is an unoptimized debug method. Production use is not recommended. </remarks>
    /// <param name="quality">JPEG: Quantization factor. PNG: ZLib compression level. 0-100</param>
    public void Save(string filename, int quality = 90, int outWidth = 0, int outHeight = 0) {
        this.ValidateNotDisposed();

        bool jpeg = filename.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    filename.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase);

        AVCodecID codec = jpeg ? AVCodecID.AV_CODEC_ID_MJPEG : AVCodecID.AV_CODEC_ID_PNG;
        AVPixelFormat pixFmt = jpeg ? AVPixelFormat.AV_PIX_FMT_YUV444P : AVPixelFormat.AV_PIX_FMT_RGBA;

        if (outWidth <= 0)
            outWidth = this.Width;
        if (outHeight <= 0)
            outHeight = this.Height;

        using (VideoEncoder encoder = new VideoEncoder(codec, new PictureFormat(outWidth, outHeight, pixFmt), 1, 10000)) {
            if (jpeg) {
                //1-31
                int q = 1 + (100 - quality) * 31 / 100;
                encoder.MaxQuantizer = q;
                encoder.MinQuantizer = q;
                encoder.Handle->color_range = AVColorRange.AVCOL_RANGE_JPEG;
            }
            else {
                //zlib compression (0-9)
                encoder.CompressionLevel = quality * 9 / 100;
            }

            encoder.Open();

            using (VideoFrame tempFrame = new VideoFrame(encoder.FrameFormat)) {
                using (SwScaler sws = new SwScaler(this.Format, tempFrame.Format)) {
                    sws.Convert(this, tempFrame);
                }

                encoder.SendFrame(tempFrame);
            }

            using (MediaPacket packet = new MediaPacket()) {
                encoder.ReceivePacket(packet);
                File.WriteAllBytes(filename, packet.Data.ToArray());
            }
        }
    }
}

/// <summary> Flags to apply to hardware frame memory mappings. </summary>
public enum HardwareFrameMappingFlags {
    /// <summary> The mapping must be readable. </summary>
    Read = 1 << 0,

    /// <summary> The mapping must be writeable. </summary>
    Write = 1 << 1,

    /// <summary>
    /// The mapped frame will be overwritten completely in subsequent
    /// operations, so the current frame data need not be loaded.  Any values
    /// which are not overwritten are unspecified.
    /// </summary>
    Overwrite = 1 << 2,

    /// <summary>
    /// The mapping must be direct.  That is, there must not be any copying in
    /// the map or unmap steps.  Note that performance of direct mappings may
    /// be much lower than normal memory.
    /// </summary>
    Direct = 1 << 3,
}

public enum HardwareFrameTransferDirection {
    /// <summary> Transfer the data from the queried hw frame. </summary>
    From = AVHWFrameTransferDirection.AV_HWFRAME_TRANSFER_DIRECTION_FROM,

    /// <summary> Transfer the data to the queried hw frame. </summary>
    To = AVHWFrameTransferDirection.AV_HWFRAME_TRANSFER_DIRECTION_TO,
}