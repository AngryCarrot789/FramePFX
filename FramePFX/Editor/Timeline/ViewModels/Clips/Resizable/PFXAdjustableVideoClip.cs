using FramePFX.Render;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Editor.Timeline.ViewModels.Clips.Resizable {
    public abstract class PFXAdjustableVideoClip : PFXVideoClip {
        protected float x;
        protected float y;
        protected float width;
        protected float height;

        public float X {
            get => this.x;
            set => this.RaisePropertyChanged(ref this.x, value);
        }

        public float Y {
            get => this.y;
            set => this.RaisePropertyChanged(ref this.y, value);
        }

        public float Width {
            get => this.width;
            set => this.RaisePropertyChanged(ref this.width, value);
        }

        public float Height {
            get => this.height;
            set => this.RaisePropertyChanged(ref this.height, value);
        }

        // for internal usage

        public bool UseScaledRender { get; set; }

        protected PFXAdjustableVideoClip() {

        }

        public void SetShape(float x, float y, float w, float h) {
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

        public override void LoadDataIntoClone(PFXBaseClip clone) {
            base.LoadDataIntoClone(clone);
            if (clone is PFXAdjustableVideoClip pos) {
                pos.x = this.x;
                pos.y = this.y;
                pos.width = this.width;
                pos.height = this.height;
                pos.UseScaledRender = this.UseScaledRender;
            }
        }
    }
}