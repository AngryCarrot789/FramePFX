using System;
using FFmpeg.AutoGen;
using FramePFX.Core.FFmpegWrapper;
using FramePFX.Core.FFmpegWrapper.Codecs;
using FramePFX.Core.FFmpegWrapper.Containers;

namespace FramePFX.Core.FFmpeg {
    public class AudioStream : StreamWrapper {
        private AudioDecoder decoder;

        protected override MediaDecoder DecoderInternal => this.decoder;

        public AudioStream(MediaStream stream) : base(stream) {
        }

        public unsafe AudioDecoder GetDecoder(bool open = true) {
            if (this.decoder == null || !this.decoder.IsOpen) {
                AVCodecID codecId = this.Stream.Handle->codecpar->codec_id;
                this.decoder = new AudioDecoder(codecId);
                int err = ffmpeg.avcodec_parameters_to_context(this.decoder.Handle, this.Stream.Handle->codecpar);
                if (FFUtils.GetException(err, "Could not copy stream parameters to the audio decoder.", out Exception e)) {
                    this.decoder.Dispose();
                    this.decoder = null;
                    throw e;
                }

                if (open) {
                    try {
                        this.decoder.Open();
                    }
                    catch {
                        this.decoder.Dispose();
                        this.decoder = null;
                        throw;
                    }
                }
            }

            return this.decoder;
        }

        public override void DisposeDecoder(bool flushBuffers = true) {
            base.DisposeDecoder(flushBuffers);
            this.decoder = null;
        }
    }
}