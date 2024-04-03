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
using FramePFX.FFmpegWrapper.Codecs;

namespace FramePFX.FFmpegWrapper.Containers
{
    public unsafe class MediaDemuxer : FFObject
    {
        private AVFormatContext* _ctx;
        private readonly bool _iocLeaveOpen;

        public AVFormatContext* Handle
        {
            get
            {
                this.ValidateNotDisposed();
                return this._ctx;
            }
        }

        public IOContext IOC { get; }

        public TimeSpan? Duration => FFUtils.GetTimeSpan(this._ctx->duration, new AVRational() { num = 1, den = ffmpeg.AV_TIME_BASE });

        public MediaStream[] Streams { get; }

        public bool CanSeek => this._ctx->pb->seek.Pointer != IntPtr.Zero;

        public MediaDemuxer(string filename) : this(filename, null)
        {
        }

        public MediaDemuxer(IOContext ioc, bool leaveOpen = false) : this(null, ioc.Handle)
        {
            this.IOC = ioc;
            this._iocLeaveOpen = leaveOpen;
        }

        private MediaDemuxer(string url, AVIOContext* pb)
        {
            this._ctx = ffmpeg.avformat_alloc_context();
            if (this._ctx == null)
            {
                throw new OutOfMemoryException("Could not allocate demuxer.");
            }

            this._ctx->pb = pb;
            fixed (AVFormatContext** c = &this._ctx)
            {
                FFUtils.CheckError(ffmpeg.avformat_open_input(c, url, null, null), "Could not open input");
            }

            FFUtils.CheckError(ffmpeg.avformat_find_stream_info(this._ctx, null), "Could not find stream information");

            this.Streams = new MediaStream[this._ctx->nb_streams];
            for (int i = 0; i < this.Streams.Length; i++)
            {
                this.Streams[i] = new MediaStream(this._ctx->streams[i]);
            }
        }

        /// <summary> Find the "best" stream in the file. The best stream is determined according to various heuristics as the most likely to be what the user expects. </summary>
        public MediaStream FindBestStream(AVMediaType type)
        {
            int index = ffmpeg.av_find_best_stream(this._ctx, type, -1, -1, null, 0);
            return index < 0 ? null : this.Streams[index];
        }

        public MediaDecoder CreateStreamDecoder(MediaStream stream, bool open = true)
        {
            if (this.Streams[stream.Index] != stream)
            {
                throw new ArgumentException("Specified stream is not owned by the demuxer.");
            }

            AVCodecID codecId = stream.Handle->codecpar->codec_id;
            MediaDecoder decoder;
            switch (stream.Type)
            {
                case MediaTypes.Audio:
                    decoder = new AudioDecoder(codecId);
                    break;
                case MediaTypes.Video:
                    decoder = new VideoDecoder(codecId);
                    break;
                default: throw new NotSupportedException($"Stream type {stream.Type} is not supported.");
            }

            FFUtils.CheckError(ffmpeg.avcodec_parameters_to_context(decoder.Handle, stream.Handle->codecpar), "Could not copy stream parameters to the decoder.");

            if (open)
            {
                decoder.Open();
            }

            return decoder;
        }

        /// <inheritdoc cref="ffmpeg.av_read_frame(AVFormatContext*, AVPacket*)"/>
        /// <returns>true if OK, false on error or end of file. On error, the packet's handle will be blank (as if it came from <see cref="ffmpeg.av_packet_alloc"/>)</returns>
        public bool Read(MediaPacket packet)
        {
            this.ValidateNotDisposed();
            int result = ffmpeg.av_read_frame(this._ctx, packet.UnrefAndGetHandle());
            if (result != 0 && result != ffmpeg.AVERROR_EOF)
            {
                throw FFUtils.GetException(result, "Failed to read next packet");
            }

            return result == 0;
        }

        /// <summary> Seeks the demuxer to some keyframe near but not later than <paramref name="timestamp"/>. </summary>
        /// <remarks> If this method returns true, all open stream decoders should be flushed by calling <see cref="CodecBase.Flush"/>. </remarks>
        /// <exception cref="InvalidOperationException">If the underlying IO context doesn't support seeks.</exception>
        public bool Seek(TimeSpan timestamp)
        {
            this.ValidateNotDisposed();

            if (!this.CanSeek)
            {
                throw new InvalidOperationException("Backing IO context is not seekable.");
            }

            long ts = ffmpeg.av_rescale(timestamp.Ticks, ffmpeg.AV_TIME_BASE, TimeSpan.TicksPerSecond);
            return ffmpeg.av_seek_frame(this._ctx, -1, ts, ffmpeg.AVSEEK_FLAG_BACKWARD) == 0;
        }

        protected override void Free()
        {
            if (this._ctx != null)
            {
                fixed (AVFormatContext** c = &this._ctx)
                    ffmpeg.avformat_close_input(c);
            }

            if (!this._iocLeaveOpen)
            {
                this.IOC?.Dispose();
            }
        }

        private void ValidateNotDisposed()
        {
            if (this._ctx == null)
            {
                throw new ObjectDisposedException(nameof(MediaDemuxer));
            }
        }
    }
}