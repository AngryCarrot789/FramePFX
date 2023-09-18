using System.Numerics;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.RBC;
using FramePFX.Rendering;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class ShapeVideoClip : VideoClip, IResourceClip<ResourceColour> {
        //, OGLRenderTarget {
        public float Width { get; set; }

        public float Height { get; set; }

        public override bool UseCustomOpacityCalculation => true;

        public ResourceHelper<ResourceColour> ResourceHelper { get; }
        BaseResourceHelper IBaseResourceClip.ResourceHelper => this.ResourceHelper;

        public ShapeVideoClip() {
            this.ResourceHelper = new ResourceHelper<ResourceColour>(this);
            this.ResourceHelper.ResourceDataModified += this.ResourceHelperOnResourceDataModified;
        }

        private void ResourceHelperOnResourceDataModified(ResourceColour resource, string property) {
            switch (property) {
                case nameof(ResourceColour.Colour):
                    this.InvalidateRender();
                    break;
            }
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetFloat(nameof(this.Width), this.Width);
            data.SetFloat(nameof(this.Height), this.Height);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Width = data.GetFloat(nameof(this.Width));
            this.Height = data.GetFloat(nameof(this.Height));
        }

        public override Vector2? GetSize() {
            return new Vector2(this.Width, this.Height);
        }

        public override bool BeginRender(long frame) {
            return this.ResourceHelper.TryGetResource(out ResourceColour _);
        }

        public override Task EndRender(RenderContext rc, long frame) {
            if (!this.ResourceHelper.TryGetResource(out ResourceColour r)) {
                return Task.CompletedTask;
            }

            // this.ApplyTransformation(rc);
            SKColor colour = RenderUtils.BlendAlpha(r.Colour, this.Opacity);
            using (SKPaint paint = new SKPaint() {Color = colour}) {
                rc.Canvas.DrawRect(0, 0, this.Width, this.Height, paint);
            }

            return Task.CompletedTask;
        }

        public void RenderGL(long frame) {
            if (!this.ResourceHelper.TryGetResource(out ResourceColour r)) {
                return;
            }

            GL.Color3(0.2f, 0.3f, 0.7f);
            GL.Vertex3(-0.5f, -0.5f, 0f);
            GL.Vertex3(0.0f, 0.5f, 0f);
            GL.Vertex3(0.5f, -0.5f, 0f);

            // SKColor colour = RenderUtils.BlendAlpha(r.Colour, this.Opacity);
            // using (SKPaint paint = new SKPaint() {Color = colour}) {
            //     rc.Canvas.DrawRect(0, 0, this.Width, this.Height, paint);
            // }
        }

        protected override Clip NewInstance() {
            return new ShapeVideoClip();
        }

        protected override void LoadDataIntoClone(Clip clone) {
            base.LoadDataIntoClone(clone);
            ShapeVideoClip clip = (ShapeVideoClip) clone;
            this.ResourceHelper.LoadDataIntoClone(clip.ResourceHelper);
            clip.Width = this.Width;
            clip.Height = this.Height;
        }
    }
}