using System;
using System.Threading.Tasks;
using FFmpeg.Wrapper;
using FramePFX.Render;
using FramePFX.ResourceManaging.Items;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Timeline.ViewModels.Clips.Resizable {
    public class VideoMediaTimelineClip : PositionableTimelineClip, IDisposable {
        private ResourceVideoMedia resource;
        public ResourceVideoMedia Resource {
            get => this.resource;
            set => this.RaisePropertyChanged(ref this.resource, value);
        }

        //TODO: move most of this stuff out to ResourceVideoMedia
        private MediaDemuxer demuxer;
        private MediaStream stream;
        private VideoDecoder decoder;

        private VideoFrame decodedFrame, downloadedHwFrame;
        private VideoFrame frameRgb;
        private SwScaler scaler;
        private Texture texture;

        private long targetFrame;
        private long currentFrameNo = -1;
        private TimeSpan frameTimestamp = TimeSpan.Zero;

        private Task catchUpTask;
        private readonly WeakReference<IViewPort> targetVp = new WeakReference<IViewPort>(null);

        public VideoMediaTimelineClip() {
            this.UseScaledRender = true;
        }

        protected void EnsureTaskRunning() {
            if (this.catchUpTask == null || this.catchUpTask.IsCompleted) {
                this.catchUpTask = Task.Run(() => {
                    long frame = this.targetFrame;
                    while (frame != this.currentFrameNo) {
                        this.ResyncFrame(frame);
                        this.currentFrameNo = frame;
                    }
                });
            }
        }

        public override void RenderCore(IViewPort vp, long frame) {
            if (this.Resource == null) {
                return;
            }

            // this.targetVp.SetTarget(vp);
            // TODO: i don't fully understand the FFmpeg library yet, but an optimisation could possibly be made for
            // TODO: the this.isTimelinePlaying field, so that it isn't seeking the frame and is instead fetching the next?
            if (frame != this.currentFrameNo) {
                // this.EnsureTaskRunning();
                this.ResyncFrame(frame);
                this.currentFrameNo = frame;
                this.targetFrame = frame;
            }

            if (this.texture == null) {
                return;
            }

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, this.texture.Id);

            GL.Begin(PrimitiveType.Quads);
            GL.Color4(1f, 1f, 1f, 1f);

            GL.TexCoord2(0, 0);
            GL.Vertex3(0, 0, 0);

            GL.TexCoord2(1, 0);
            GL.Vertex3(1, 0, 0);

            GL.TexCoord2(1, 1);
            GL.Vertex3(1, 1, 0);

            GL.TexCoord2(0, 1);
            GL.Vertex3(0, 1, 0);

            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }

        private void OpenResource() {
            this.demuxer = new MediaDemuxer(this.Resource.FilePath);

            this.stream = this.demuxer.FindBestStream(MediaTypes.Video);
            this.decoder = (VideoDecoder) this.demuxer.CreateStreamDecoder(this.stream, open: false);

            this.TrySetupHardwareDecoder();
            this.decoder.Open();

            //Select the smallest size from either clip or source for our temp frames
            int frameW = (int) Math.Round(this.Width);
            int frameH = (int) Math.Round(this.height);

            if (frameW > this.decoder.Width || frameH > this.decoder.Height) {
                frameW = this.decoder.Width;
                frameH = this.decoder.Height;
            }

            this.decodedFrame = new VideoFrame(this.decoder.FrameFormat);
            this.frameRgb = new VideoFrame(frameW, frameH, PixelFormats.RGBA);
            this.texture = new Texture(frameW, frameH);
        }

        private void TrySetupHardwareDecoder() {
            foreach (CodecHardwareConfig config in this.decoder.GetHardwareConfigs()) {
                using (var device = HardwareDevice.Create(config.DeviceType)) {
                    if (device != null) {
                        this.decoder.SetupHardwareAccelerator(device, config.PixelFormat);
                        break;
                    }
                }
            }
        }

        private void ResyncFrame(long frameNo) {
            if (this.demuxer == null) {
                this.OpenResource();
            }

            //TODO: We'll miss frames depending on the project/source framerates -- add frame interpolation?
            double timeScale = this.Layer.Timeline.Project.PlaybackFPS;
            TimeSpan timestamp = TimeSpan.FromSeconds((frameNo - this.FrameBegin) / timeScale);
            if (this.SeekToFrame(timestamp)) {
                this.UploadFrame();
            }
        }

        private void ResyncFrameAsync(long frameNo) {
            // force must be true, otherwise there's never a time where it can access the OGL context
            if (this.targetVp.TryGetTarget(out IViewPort vp) && vp.Context.BeginUse(true)) {
                try {
                    this.ResyncFrame(frameNo);
                }
                finally {
                    vp.Context.EndUse();
                }
            }
        }

        private bool SeekToFrame(TimeSpan timestamp) {
            //Images have a single frame, which we'll miss if we don't clamp timestamp to zero.
            TimeSpan duration = this.demuxer.Duration ?? TimeSpan.Zero;
            if (timestamp > duration) {
                timestamp = duration;
            }

            //If the last decoded frame is past or too far after the requested timestamp, seek to the nearest keyframe before it
            double frameDist = (timestamp - this.frameTimestamp).TotalSeconds;
            if (frameDist < -1.0 / this.stream.AvgFrameRate || frameDist > 5.0) {
                this.demuxer.Seek(timestamp);
                this.decoder.Flush();
            }

            //Decode frames until we find one that is close to the requested timestamp
            while (true) {
                if (this.decoder.ReceiveFrame(this.decodedFrame)) {
                    long? t = this.decodedFrame.PresentationTimestamp;
                    this.frameTimestamp = this.stream.GetTimestamp(t.Value);
                    if (this.frameTimestamp >= timestamp) {
                        return true;
                    }
                }

                //Fill-up the decoder with more data and try again
                using (var packet = new MediaPacket()) {
                    bool gotPacket = false;
                    while (this.demuxer.Read(packet)) {
                        if (packet.StreamIndex == this.stream.Index) {
                            // TODO: this throws when you try to play back .mkv
                            this.decoder.SendPacket(packet);
                            gotPacket = true;
                            break;
                        }
                    }

                    if (!gotPacket) {
                        //Reached the end of the file
                        return false;
                    }
                }
            }
        }

        private void UploadFrame() {
            VideoFrame frame = this.decodedFrame;

            if (frame.IsHardwareFrame) {
                //As of ffmpeg 6.0, GetHardwareTransferFormats() only returns more than one format for VAAPI,
                //which isn't widely supported on Windows yet, so we can't transfer directly to RGB without
                //hacking into the API specific device context (like D3D11VA).
                frame.TransferTo(this.downloadedHwFrame ?? (this.downloadedHwFrame = new VideoFrame()));
                frame = this.downloadedHwFrame;
            }

            if (this.scaler == null) {
                this.scaler = new SwScaler(frame.Format, this.frameRgb.Format);
            }

            this.scaler.Convert(frame, this.frameRgb);

            Span<byte> pixelData = this.frameRgb.GetPlaneSpan<byte>(0, out int rowBytes);
            // if (this.targetVp.TryGetTarget(out IViewPort viewPort)) {
            //
            // }
            this.texture.SetPixels<byte>(pixelData, 0, 0, this.frameRgb.Width, this.frameRgb.Height, PixelFormat.Rgba, PixelType.UnsignedByte, rowBytes / 4);
        }

        protected override void DisposeClip() {
            base.DisposeClip();
            this.demuxer?.Dispose();
            this.decoder?.Dispose();
            this.decodedFrame?.Dispose();
            this.downloadedHwFrame?.Dispose();
            this.frameRgb?.Dispose();
            this.scaler?.Dispose();
        }

        public override BaseTimelineClip CloneInstance() {
            VideoMediaTimelineClip clip = new VideoMediaTimelineClip();
            this.LoadDataIntoClone(clip);
            return clip;
        }

        public override void LoadDataIntoClone(BaseTimelineClip clone) {
            base.LoadDataIntoClone(clone);
            if (clone is VideoMediaTimelineClip clip) {
                clip.resource = this.resource;
            }
        }
    }
}