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
using FramePFX.FFmpegWrapper.Containers;

namespace FramePFX.FFmpegWrapper;

public unsafe class MediaPacket : FFObject
{
    internal AVPacket* packet;

    public AVPacket* Handle
    {
        get
        {
            this.ValidateNotDisposed();
            return this.packet;
        }
    }

    /// <summary>
    /// Presentation timestamp in <see cref="MediaStream.TimeBase"/> units;
    /// the time at which the decompressed packet will be presented to the user. <br/>
    ///
    /// Can be <see langword="null"/> if it is not stored in the file. MUST be larger
    /// or equal to <see cref="DecompressionTimestamp"/> as presentation cannot happen before
    /// decompression, unless one wants to view hex dumps.  <br/>
    ///
    /// Some formats misuse the terms dts and pts/cts to mean something different.
    /// Such timestamps must be converted to true pts/dts before they are stored in AVPacket.
    /// </summary>
    public long? PresentationTimestamp
    {
        get => FFUtils.GetPTS(this.packet->pts);
        set => FFUtils.SetPTS(ref this.packet->pts, value);
    }

    public long? DecompressionTimestamp
    {
        get => FFUtils.GetPTS(this.packet->dts);
        set => FFUtils.SetPTS(ref this.packet->dts, value);
    }

    /// <summary> Duration of this packet in <see cref="MediaStream.TimeBase"/> units, 0 if unknown. Equals next_pts - this_pts in presentation order.  </summary>
    public long Duration
    {
        get => this.packet->duration;
        set => this.packet->duration = value;
    }

    public int StreamIndex
    {
        get => this.packet->stream_index;
        set => this.packet->stream_index = value;
    }

    public Span<byte> Data => new Span<byte>(this.packet->data, this.packet->size);

    public MediaPacket()
    {
        this.packet = ffmpeg.av_packet_alloc();
    }

    /// <inheritdoc cref="ffmpeg.av_packet_rescale_ts(AVPacket*, AVRational, AVRational)"/>
    public void RescaleTS(AVRational sourceBase, AVRational destBase)
    {
        ffmpeg.av_packet_rescale_ts(this.Handle, sourceBase, destBase);
    }

    /// <summary> Returns the underlying packet pointer after calling av_packet_unref() on it. </summary>
    public AVPacket* UnrefAndGetHandle()
    {
        this.ValidateNotDisposed();
        ffmpeg.av_packet_unref(this.packet);
        return this.packet;
    }

    protected override void Free()
    {
        fixed (AVPacket** pkt = &this.packet)
        {
            ffmpeg.av_packet_free(pkt);
        }
    }

    private void ValidateNotDisposed()
    {
        if (this.packet == null)
        {
            throw new ObjectDisposedException(nameof(MediaPacket));
        }
    }
}