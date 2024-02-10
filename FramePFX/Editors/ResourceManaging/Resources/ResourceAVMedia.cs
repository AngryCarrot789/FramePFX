using System;
using System.IO;
using FFmpeg.AutoGen;
using FramePFX.Editors.ResourceManaging.Autoloading;
using FramePFX.Editors.ResourceManaging.Events;
using FramePFX.FFmpeg;
using FramePFX.FFmpegWrapper;
using FramePFX.FFmpegWrapper.Codecs;
using FramePFX.FFmpegWrapper.Containers;
using FramePFX.Logger;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editors.ResourceManaging.Resources {
    /// <summary>
    /// A audio-visual media resource. This handles a cached collection of decoders
    /// </summary>
    public class ResourceAVMedia : ResourceItem {
        private string filePath;
        private string loadedFilePath;

        public string FilePath {
            get => this.filePath;
            set {
                if (this.filePath == value)
                    return;
                this.filePath = value;
                this.FilePathChanged?.Invoke(this);
            }
        }

        public MediaDemuxer Demuxer;
        public MediaStream stream;
        public VideoDecoder decoder;
        public FrameQueue frameQueue;
        public bool hasHardwareDecoder;

        public event ResourceEventHandler FilePathChanged;

        public ResourceAVMedia() {

        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            if (!string.IsNullOrEmpty(this.filePath))
                data.SetString(nameof(this.FilePath), this.filePath);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.filePath = data.GetString(nameof(this.FilePath), null);
        }

        protected override bool OnTryAutoEnable(ResourceLoader loader) {
            if (string.IsNullOrEmpty(this.FilePath)) {
                return true;
            }

            try {
                this.LoadMediaFile();
            }
            catch (Exception e) {
                loader?.AddEntry(new InvalidMediaPathEntry(this, e));
                return false;
            }

            return true;
        }

        public unsafe Vec2i? GetResolution() {
            if (this.stream != null) {
                //Wrappers don't expose this stuff so :')
                AVCodecParameters* pars = this.stream.Handle->codecpar;
                return new Vec2i(pars->width, pars->height);
            }

            return null;
        }

        public TimeSpan GetDuration() {
            return this.Demuxer.Duration ?? TimeSpan.Zero;
        }

        public VideoFrame GetFrameAt(TimeSpan timestamp, out double minDistanceToTimeStampSecs) {
            if (this.stream == null || this.decoder == null) {
                minDistanceToTimeStampSecs = 0d;
                return null;
            }

            // Images have a single frame, which we'll miss if we don't clamp timestamp to zero.
            if (timestamp > this.GetDuration()) {
                timestamp = this.GetDuration();
            }

            VideoFrame frame = this.frameQueue.GetNearest(timestamp, out minDistanceToTimeStampSecs);
            double distThreshold = 0.5 / this.stream.AvgFrameRate; //will dupe too many frames if set to 1.0

            if (Math.Abs(minDistanceToTimeStampSecs) > distThreshold) {
                // If the nearest frame is past or too far after the requested timestamp, seek to the nearest keyframe before it
                if (minDistanceToTimeStampSecs < 0 || minDistanceToTimeStampSecs > 3.0) {
                    this.Demuxer.Seek(timestamp);
                    this.decoder.Flush();
                }

                this.DecodeFramesUntil(timestamp);
                frame = this.frameQueue.GetNearest(timestamp, out minDistanceToTimeStampSecs);
            }

            return frame;
        }

        private bool DecodeFramesUntil(TimeSpan endTime) {
            while (true) {
                VideoFrame frame = this.frameQueue.Current();
                if (this.decoder.ReceiveFrame(frame)) {
                    if (this.frameQueue.Shift(out long spanTicks) && spanTicks >= endTime.Ticks) {
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

        public void LoadMediaFile() {
            this.DisposeMediaFile();
            try {
                this.Demuxer = new MediaDemuxer(this.FilePath);
                this.stream = this.Demuxer.FindBestStream(MediaTypes.Video);
            }
            catch (Exception e) {
                this.DisposeMediaFile();
                // Invalid media file
                throw new IOException("Failed to open demuxer", e);
            }

            try {
                this.decoder = (VideoDecoder) this.Demuxer.CreateStreamDecoder(this.stream, false);
                this.frameQueue = new FrameQueue(this.stream, 8);
                this.hasHardwareDecoder = this.TrySetupHardwareDecoder();
                this.decoder.Open();
            }
            catch (Exception e) {
                this.DisposeMediaFile();
                throw new Exception("Could not open decoder", e);
            }

            if (this.stream == null) {
                this.DisposeMediaFile();
                throw new Exception("Could not find a video stream for media");
            }

            this.loadedFilePath = this.FilePath;
        }

        public override void Destroy() {
            base.Destroy();
            this.DisposeMediaFile();
        }

        public void DisposeMediaFile() {
            try {
                this.Demuxer?.Dispose();
            }
            catch (Exception e) {
                AppLogger.Instance.WriteLine("Error disposing demuxer: " + e.GetToString());
            }

            try {
                this.decoder?.Dispose();
            }
            catch (Exception e) {
                AppLogger.Instance.WriteLine("Error disposing decoder: " + e.GetToString());
            }

            try {
                this.frameQueue?.Dispose();
            }
            catch (Exception e) {
                AppLogger.Instance.WriteLine("Error disposing frame queue: " + e.GetToString());
            }

            this.Demuxer = null;
            this.decoder = null;
            this.frameQueue = null;
            this.hasHardwareDecoder = false;
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
    }
}