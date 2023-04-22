using System;
using System.Threading.Tasks;
using FFmpeg.Wrapper;
using FramePFX.Core.Utils;
using FramePFX.Render;
using FramePFX.ResourceManaging.Items;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Editor.Timeline.ViewModels.Clips.Resizable {
    public class PFXMPEGMediaClip : PFXAdjustableVideoClip, IDisposable {
        private ResourceMedia resource;
        public ResourceMedia Resource {
            get => this.resource;
            set => this.RaisePropertyChanged(ref this.resource, value);
        }
        
        private VideoFrame frameRgb, downloadedHwFrame;
        private SwScaler scaler;
        private Texture texture;

        private long targetFrame;
        private long currentFrameNo = -1;
        private TimeSpan frameTimestamp = TimeSpan.Zero;

        private Task catchUpTask;
        private readonly WeakReference<IViewPort> targetVp = new WeakReference<IViewPort>(null);

        public PFXMPEGMediaClip() {
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

            if (this.texture == null) {
                this.CreateTexture(vp.Context);
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

        private void CreateTexture(IRenderContext context) {
            //Select the smallest size from either clip or source for our temp frames
            Resolution sourceRes = this.Resource.GetResolution();

            int frameW = (int) Math.Round(this.Width);
            int frameH = (int) Math.Round(this.height);

            if (frameW > sourceRes.Width || frameH > sourceRes.Height) {
                frameW = sourceRes.Width;
                frameH = sourceRes.Height;
            }
            this.frameRgb = new VideoFrame(frameW, frameH, PixelFormats.RGBA);
            this.texture = new Texture(context, frameW, frameH);
        }

        private void ResyncFrame(long frameNo) {
            if (this.texture == null) {
                return;
            }

            double timeScale = this.Layer.Timeline.Project.FrameRate;
            TimeSpan timestamp = TimeSpan.FromSeconds((frameNo - this.FrameBegin + this.FrameMediaOffset) / timeScale);
            VideoFrame frame = this.Resource.GetFrameAt(timestamp);

            if (frame != null) {
                this.UploadFrame(frame);
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

        private void UploadFrame(VideoFrame frame) {
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

        protected override void DisposeClip(ExceptionStack stack) {
            base.DisposeClip(stack);
            try {
                this.downloadedHwFrame?.Dispose();
            }
            catch(Exception e) {
                stack.Add(new Exception("Exception while disposing downloaded frame", e));
            }

            try {
                this.frameRgb?.Dispose();
            }
            catch (Exception e) {
                stack.Add(new Exception("Exception while disposing current frame", e));
            }

            try {
                this.scaler?.Dispose();
            }
            catch (Exception e) {
                stack.Add(new Exception("Exception while disposing scaler", e));
            }

            if (this.texture == null) {
                return;
            }

            if (!this.texture.Context.BeginUse(false)) {
                stack.Add(new Exception("Failed to use texture context"));
            }
            else {
                try {
                    this.texture.Dispose();
                }
                catch (Exception e) {
                    stack.Add(new Exception("Exception while disposing scaler", e));
                }
            }
        }

        public override PFXBaseClip NewInstanceOverride() {
            return new PFXMPEGMediaClip();
        }

        public override void LoadDataIntoClone(PFXBaseClip clone) {
            base.LoadDataIntoClone(clone);
            if (clone is PFXMPEGMediaClip clip) {
                clip.resource = this.resource;
            }
        }
    }
}