using System;
using System.IO;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using FFmpeg.Wrapper;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging.Resources {
    public class ResourceMedia : ResourceItem {
        public string FilePath { get; set; }

        public MediaDemuxer Demuxer { get; private set; }

        public bool IsValidMediaFile => this.Demuxer != null;

        private MediaStream stream;
        private VideoDecoder decoder;
        private FrameQueue frameQueue;
        private bool hasHardwareDecoder;

        public ResourceMedia() {

        }

        protected override Task DisableCoreAsync(ExceptionStack stack, bool user) {
            this.DisposeCore(stack);
            return Task.CompletedTask;
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetString(nameof(this.FilePath), this.FilePath);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.FilePath = data.GetString(nameof(this.FilePath), null);
        }

        public void OpenMediaFromFile() {
            this.OpenFileAndDemuxer();
        }

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
                VideoFrame frame = this.frameQueue.Reserve();
                if (this.decoder.ReceiveFrame(frame)) {
                    if (this.frameQueue.Shift() >= endTime) {
                        return true;
                    }
                }

                // Fill-up the decoder with more data and try again
                using (var packet = new MediaPacket()) {
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
                this.ReleaseDecoder();
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
                using (var device = HardwareDevice.Create(config.DeviceType)) {
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
        public void ReleaseDecoder() {
            this.decoder?.Dispose();
            this.decoder = null;

            this.frameQueue?.Dispose();
            this.frameQueue = null;

            this.hasHardwareDecoder = false;
        }

        public void CloseFile() {
            this.ReleaseDecoder();
            this.Demuxer?.Dispose();
            this.Demuxer = null;
        }

        protected override void DisposeCore(ExceptionStack stack) {
            base.DisposeCore(stack);
            try {
                this.ReleaseDecoder();
            }
            catch (Exception e) {
                stack.Add(new Exception("Failed to release decoder and frame queue", e));
            }

            try {
                this.Demuxer?.Dispose();
            }
            catch (Exception e) {
                stack.Add(new Exception("Failed to dispose demuxer", e));
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

        //Rolling circular-buffer of `VideoFrame`s
        private class FrameQueue : IDisposable {
            private readonly MediaStream stream;
            private readonly VideoFrame[] frames;
            private int index = 0;

            public FrameQueue(MediaStream stream, int size) {
                this.stream = stream;
                this.frames = new VideoFrame[size];

                for (int i = 0; i < size; i++) {
                    this.frames[i] = new VideoFrame();
                }
            }

            public VideoFrame Reserve() {
                return this.frames[this.index % this.frames.Length];
            }

            public TimeSpan Shift() {
                VideoFrame lastFrame = this.Reserve();
                this.index++;
                return this.GetTime(lastFrame);
            }

            public TimeSpan GetTime(VideoFrame frame) {
                return this.stream.GetTimestamp(frame.PresentationTimestamp.Value);
            }

            public VideoFrame GetNearest(TimeSpan timestamp, out double nearestDist) {
                VideoFrame nearestFrame = null;
                nearestDist = double.PositiveInfinity;
                foreach (VideoFrame frame in this.frames) {
                    unsafe {
                        if (frame.Handle == null || frame.PresentationTimestamp == null)
                            continue;
                    }

                    double dist = (timestamp - this.GetTime(frame)).TotalSeconds;
                    if (Math.Abs(dist) < Math.Abs(nearestDist)) {
                        nearestFrame = frame;
                        nearestDist = dist;
                    }
                }
                return nearestFrame;
            }

            public void Dispose() {
                for (int i = 0; i < this.frames.Length; i++) {
                    this.frames[i].Dispose();
                }
            }
        }
    }
}