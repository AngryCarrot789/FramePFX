using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.RBC;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using OpenTK.Graphics.OpenGL;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timelines.VideoClips {
    public class ShapeVideoClip : BaseResourceVideoClip<ResourceColour> {//, OGLRenderTarget {
        public float Width { get; set; }

        public float Height { get; set; }

        public override bool UseCustomOpacityCalculation => true;

        // public override bool UseAsyncRendering => true;

        public ShapeVideoClip() {
        }

        protected override void OnResourceDataModified(string property) {
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

        public override void Render(RenderContext rc, long frame) {
            if (!this.TryGetResource(out ResourceColour r)) {
                return;
            }

            this.Transform(rc);
            SKColor colour = RenderUtils.BlendAlpha(r.Colour, this.Opacity);
            using (SKPaint paint = new SKPaint() {Color = colour}) {
                rc.Canvas.DrawRect(0, 0, this.Width, this.Height, paint);
            }
        }

        public void RenderGL(long frame) {
            if (!this.TryGetResource(out ResourceColour r)) {
                return;
            }
            
            GL.Color3(0.2f, 0.3f, 0.7f);
            GL.Vertex3(-0.5f, -0.5f, 0f);
            GL.Vertex3( 0.0f,  0.5f, 0f);
            GL.Vertex3( 0.5f, -0.5f, 0f);

            // SKColor colour = RenderUtils.BlendAlpha(r.Colour, this.Opacity);
            // using (SKPaint paint = new SKPaint() {Color = colour}) {
            //     rc.Canvas.DrawRect(0, 0, this.Width, this.Height, paint);
            // }
        }

        private long ff;
        public override void BeginRender(long frame) {
            Task.Run(async () => {
                await Task.Delay(2000);
                this.IsAsyncRenderReady = true;
            });

            this.ff = frame;
        }

        public override void EndRender(RenderContext rc) {
            base.EndRender(rc);
            this.Render(rc, this.ff);
        }

        protected override Clip NewInstance() {
            return new ShapeVideoClip();
        }

        protected override void LoadDataIntoClone(Clip clone) {
            base.LoadDataIntoClone(clone);
            ShapeVideoClip clip = (ShapeVideoClip) clone;
            clip.Width = this.Width;
            clip.Height = this.Height;
        }
    }
}