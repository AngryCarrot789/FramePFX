using FramePFX.Render;
using FramePFX.ResourceManaging.Items;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Editor.Timeline.New.Clips {
    public class PFXShapeClip : PFXAdjustableClip {
        public ResourceRGBA Resource { get; set; }

        public float R => this.Resource?.R ?? 0f;

        public float G => this.Resource?.G ?? 0f;

        public float B => this.Resource?.B ?? 0f;

        public float A => this.Resource?.A ?? 1f;

        public PFXShapeClip() {
            this.UseScaledRender = true;
        }

        protected override PFXClip NewInstanceCore() {
            return new PFXShapeClip();
        }

        public override void RenderCore(IViewPort vp, long frame) {
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(this.R, this.G, this.B, this.A);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(1, 0, 0);
            GL.Vertex3(1, 1, 0);
            GL.Vertex3(0, 1, 0);
            GL.End();
        }

        public override void LoadDataIntoClone(PFXClip clone) {
            base.LoadDataIntoClone(clone);
            if (clone is PFXShapeClip shape) {
                shape.Resource = this.Resource;
            }
        }
    }
}