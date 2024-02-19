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
using System.Collections.Generic;
using System.Linq;
using FFmpeg.AutoGen;
using FramePFX.FFmpegWrapper.Codecs;

namespace FramePFX.FFmpegWrapper.Containers {
    public unsafe class MediaMuxer : FFObject {
        private AVFormatContext* _ctx;

        public AVFormatContext* Handle {
            get {
                this.ValidateNotDisposed();
                return this._ctx;
            }
        }

        public IOContext IOC { get; }
        private bool _iocLeaveOpen;

        private List<(MediaStream Stream, MediaEncoder Encoder)> _streams = new List<(MediaStream Stream, MediaEncoder Encoder)>();
        private MediaPacket _tempPacket;

        public IReadOnlyList<MediaStream> Streams => this._streams.Select(s => s.Stream).ToList();

        public bool IsOpen { get; private set; } = false;

        public MediaMuxer(string filename) {
            fixed (AVFormatContext** fmtCtx = &this._ctx) {
                FFUtils.CheckError(ffmpeg.avformat_alloc_output_context2(fmtCtx, null, null, filename), "Could not allocate muxer");
            }

            FFUtils.CheckError(ffmpeg.avio_open(&this._ctx->pb, filename, ffmpeg.AVIO_FLAG_WRITE), "Could not open output file");
        }

        public MediaMuxer(IOContext ioc, string formatExtension, bool leaveOpen = false) : this(ioc, ContainerTypes.GetOutputFormat(formatExtension), leaveOpen) {
        }

        public MediaMuxer(IOContext ioc, AVOutputFormat* format, bool leaveOpen = false) {
            this.IOC = ioc;
            this._iocLeaveOpen = leaveOpen;

            this._ctx = ffmpeg.avformat_alloc_context();
            if (this._ctx == null) {
                throw new OutOfMemoryException("Could not allocate muxer");
            }

            this._ctx->oformat = format;
            this._ctx->pb = ioc.Handle;
        }

        /// <summary> Creates and adds a new stream to the muxed file. </summary>
        /// <remarks> The <paramref name="encoder"/> must be open before this is called. </remarks>
        public MediaStream AddStream(MediaEncoder encoder) {
            this.ValidateNotDisposed();
            if (this.IsOpen) {
                throw new InvalidOperationException("Cannot add new streams once the muxer is open.");
            }

            if (encoder.IsOpen) {
                //This is an unfortunate limitation, but the GlobalHeader flag must be set before the encoder is open.
                throw new InvalidOperationException("Cannot add stream with an already open encoder.");
            }

            AVStream* stream = ffmpeg.avformat_new_stream(this._ctx, encoder.Handle->codec);
            if (stream == null) {
                throw new OutOfMemoryException("Could not allocate stream");
            }

            stream->id = (int) this._ctx->nb_streams - 1;
            stream->time_base = encoder.TimeBase;

            //Some formats want stream headers to be separate.
            if ((this._ctx->oformat->flags & ffmpeg.AVFMT_GLOBALHEADER) != 0) {
                encoder.Handle->flags |= ffmpeg.AV_CODEC_FLAG_GLOBAL_HEADER;
            }

            MediaStream st = new MediaStream(stream);
            this._streams.Add((st, encoder));
            return st;
        }

        /// <summary> Opens all streams and writes the container header. </summary>
        /// <remarks> This method will also open all encoders passed to <see cref="AddStream(MediaEncoder)"/>. </remarks>
        public void Open() {
            this.ValidateNotDisposed();
            if (this.IsOpen) {
                throw new InvalidOperationException("Muxer is already open.");
            }

            foreach ((MediaStream stream, MediaEncoder encoder) in this._streams) {
                encoder.Open();
                FFUtils.CheckError(ffmpeg.avcodec_parameters_from_context(stream.Handle->codecpar, encoder.Handle), "Could not copy the encoder parameters to the stream.");
            }

            FFUtils.CheckError(ffmpeg.avformat_write_header(this._ctx, null), "Could not write header to output file");
            this.IsOpen = true;
        }

        public void Write(MediaPacket packet) {
            this.ThrowIfNotOpen();

            FFUtils.CheckError(ffmpeg.av_interleaved_write_frame(this._ctx, packet.Handle), "Failed to write frame");
        }

        public void EncodeAndWrite(MediaStream stream, MediaEncoder encoder, MediaFrame frame) {
            this.ThrowIfNotOpen();

            if (this._streams[stream.Index].Stream != stream) {
                throw new ArgumentException("Specified stream is not owned by the muxer.");
            }

            if (this._tempPacket == null)
                this._tempPacket = new MediaPacket();

            encoder.SendFrame(frame);

            while (encoder.ReceivePacket(this._tempPacket)) {
                this._tempPacket.RescaleTS(encoder.TimeBase, stream.TimeBase);
                this._tempPacket.StreamIndex = stream.Index;
                ffmpeg.av_interleaved_write_frame(this._ctx, this._tempPacket.Handle);
            }
        }

        private void ThrowIfNotOpen() {
            this.ValidateNotDisposed();

            if (!this.IsOpen) {
                throw new InvalidOperationException("Muxer is not open");
            }
        }

        protected override void Free() {
            if (this._ctx != null) {
                ffmpeg.av_write_trailer(this._ctx);
                ffmpeg.avformat_free_context(this._ctx);
                this._ctx = null;

                if (!this._iocLeaveOpen) {
                    this.IOC?.Dispose();
                }

                this._tempPacket?.Dispose();
            }
        }

        private void ValidateNotDisposed() {
            if (this._ctx == null) {
                throw new ObjectDisposedException(nameof(MediaMuxer));
            }
        }
    }
}