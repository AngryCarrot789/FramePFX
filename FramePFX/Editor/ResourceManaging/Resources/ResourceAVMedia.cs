using System;
using System.IO;
using FFmpeg.AutoGen;
using FramePFX.FFmpeg;
using FramePFX.FFmpegWrapper;
using FramePFX.FFmpegWrapper.Codecs;
using FramePFX.FFmpegWrapper.Containers;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging.Resources {
    public class ResourceAVMedia : ResourceItem {
        public string FilePath { get; set; }

        public MediaDemuxer Demuxer { get; private set; }

        public bool IsValidMediaFile => this.Demuxer != null;

        internal MediaStream stream;
        private VideoDecoder decoder;
        private FrameQueue frameQueue;
        private bool hasHardwareDecoder;

        public ResourceAVMedia() {
        }

        protected override void OnDisableCore(ErrorList list, bool user) {
            base.OnDisableCore(list, user);
            this.CloseFile(list);
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetString(nameof(this.FilePath), this.FilePath);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.FilePath = data.GetString(nameof(this.FilePath), null);
        }

        public void OpenMediaFromFile() => this.OpenFileAndDemuxer();

        public unsafe Resolution? GetResolution() {
            if (this.stream != null) {
                //Wrappers don't expose this stuff so :')
                AVCodecParameters* pars = this.stream.Handle->codecpar;
                return new Resolution(pars->width, pars->height);
            }

            return null;
        }

        public TimeSpan GetDuration() {
            return this.Demuxer.Duration ?? TimeSpan.Zero;
        }

        public VideoFrame GetFrameAt(TimeSpan timestamp) {
            if (this.stream == null) {
                return null;
            }

            if (this.decoder == null) {
                this.OpenVideoDecoder();
            }

            //Images have a single frame, which we'll miss if we don't clamp timestamp to zero.
            if (timestamp > this.GetDuration()) {
                timestamp = this.GetDuration();
            }

            VideoFrame frame = this.frameQueue.GetNearest(timestamp, out double frameDist);
            double distThreshold = 0.5 / this.stream.AvgFrameRate; //will dupe too many frames if set to 1.0

            if (Math.Abs(frameDist) > distThreshold) {
                //If the nearest frame is past or too far after the requested timestamp, seek to the nearest keyframe before it
                if (frameDist < 0 || frameDist > 5.0) {
                    this.Demuxer.Seek(timestamp);
                    this.decoder.Flush();
                }

                this.DecodeFramesUntil(timestamp);
                frame = this.frameQueue.GetNearest(timestamp, out _);
            }

            return frame;
        }

        private bool DecodeFramesUntil(TimeSpan endTime) {
            while (true) {
                VideoFrame frame = this.frameQueue.Current();
                if (this.decoder.ReceiveFrame(frame)) {
                    if (this.frameQueue.Shift() is TimeSpan ts && ts >= endTime) {
                        return true;
                    }
                }

                // Fill-up the decoder with more data and try again
                using (MediaPacket packet = new MediaPacket()) {
                    bool gotPacket = false;
                    while (this.Demuxer.Read(packet)) {
                        if (packet.StreamIndex == this.stream.Index) {
                            this.decoder.SendPacket(packet);
                            gotPacket = true;
                            break;
                        }
                    }

                    if (!gotPacket) {
                        // Reached the end of the file
                        return false;
                    }
                }
            }
        }

        private void OpenFileAndDemuxer() {
            try {
                this.CloseDecoder();
                this.Demuxer = new MediaDemuxer(this.FilePath);
                this.stream = this.Demuxer.FindBestStream(MediaTypes.Video);
            }
            catch (Exception e) {
                try {
                    this.Dispose();
                }
                catch (Exception ex) {
                    e.AddSuppressed(ex);
                }

                // Invalid media file
                throw new IOException("Failed to open demuxer", e);
            }

            if (this.stream == null) {
                throw new Exception("Could not find a video stream for media");
            }

            this.IsOnline = true;
        }

        public void OpenVideoDecoder() {
            this.EnsureHasVideoStream();
            this.decoder = (VideoDecoder) this.Demuxer.CreateStreamDecoder(this.stream, open: false);
            this.frameQueue = new FrameQueue(this.stream, 4);
            this.hasHardwareDecoder = this.TrySetupHardwareDecoder();
            this.decoder.Open();
        }

        private bool TrySetupHardwareDecoder() {
            if (this.decoder.PixelFormat != AVPixelFormat.AV_PIX_FMT_YUV420P &&
                this.decoder.PixelFormat != AVPixelFormat.AV_PIX_FMT_YUV420P10LE) {
                //TODO: SetupHardwareAccelerator() will return NONE from get_format() rather than fallback to sw formats,
                //      causing SendPacket() to throw with InvalidData for sources with unsupported hw pixel formats like YUV444.
                return false;
            }

            foreach (CodecHardwareConfig config in this.decoder.GetHardwareConfigs()) {
                using (HardwareDevice device = HardwareDevice.Create(config.DeviceType)) {
                    if (device != null) {
                        this.decoder.SetupHardwareAccelerator(device, config.PixelFormat);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Deallocates media decoders and internal frames.
        /// </summary>
        public void CloseDecoder() {
            this.decoder?.Dispose();
            this.decoder = null;

            this.frameQueue?.Dispose();
            this.frameQueue = null;

            this.hasHardwareDecoder = false;
        }

        public void CloseFile() {
            this.CloseDecoder();
            this.Demuxer?.Dispose();
            this.Demuxer = null;
        }

        protected override void DisposeCore(ErrorList list) {
            base.DisposeCore(list);
            this.CloseFile(list);
        }

        private void CloseFile(ErrorList list) {
            try {
                this.CloseDecoder();
            }
            catch (Exception e) {
                list.Add(new Exception("Failed to release decoder and frame queue", e));
            }

            try {
                this.Demuxer?.Dispose();
            }
            catch (Exception e) {
                list.Add(new Exception("Failed to dispose demuxer", e));
            }
            finally {
                this.Demuxer = null;
            }
        }

        protected void EnsureHasVideoStream() {
            if (this.stream == null) {
                throw new InvalidOperationException("No video stream is available");
            }
        }
    }
}