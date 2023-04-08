using FramePFX.Render;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Timeline.ViewModels.Clips.Resizable {
    public abstract class PositionableClipViewModel : VideoClipViewModel {
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

        public bool UseScaledRender { get; set; }

        protected PositionableClipViewModel() {
            this.UseScaledRender = true;
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

            this.RenderCore(vp, frame);
            GL.PopMatrix();
        }

        public abstract void RenderCore(IViewPort vp, long frame);
    }
}