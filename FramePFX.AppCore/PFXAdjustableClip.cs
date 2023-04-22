using FramePFX.Editor.Timeline.New.Layers;
using FramePFX.Render;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Editor.Timeline.New.Clips {
    public abstract class PFXAdjustableClip : PFXVideoClip {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public bool UseScaledRender { get; protected set; }

        protected PFXAdjustableClip() {

        }

        public void SetXYWH(float x, float y, float w, float h) {
            this.X = x;
            this.Y = y;
            this.Width = w;
            this.Height = h;
        }

        public void TranslateForScaledRender(IViewPort ogl) {
            GL.Translate(this.X, this.Y, 0f);
            GL.Scale(ogl.Width * (this.Width / ogl.Width), ogl.Height * (this.Height / ogl.Height), 1f);
            // GL.Rotate(this.rotZ, 0, 0, 1);
        }

        public override void Render(IViewPort vp, long frame) {
            GL.PushMatrix();
            if (this.UseScaledRender) {
                this.TranslateForScaledRender(vp);
            }

            System.Diagnostics.Debug.WriteLine("Rendered frame " + frame);
            this.RenderCore(vp, frame);
            GL.PopMatrix();
        }

        /// <summary>
        /// The core render function for a positionable timeline clip
        /// </summary>
        /// <param name="vp">The viewport that's being rendered into</param>
        /// <param name="frame">The current frame that needs to be rendered</param>
        public abstract void RenderCore(IViewPort vp, long frame);

        public override void LoadDataIntoClone(PFXClip clone) {
            base.LoadDataIntoClone(clone);
            if (clone is PFXAdjustableClip pos) {
                pos.X = this.X;
                pos.Y = this.Y;
                pos.Width = this.Width;
                pos.Height = this.Height;
                pos.UseScaledRender = this.UseScaledRender;
            }
        }
    }
}