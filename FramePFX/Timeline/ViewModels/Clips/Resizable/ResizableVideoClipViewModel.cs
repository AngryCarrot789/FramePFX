using FramePFX.Render;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Timeline.ViewModels.Clips.Resizable {
    public abstract class UIResizableVideoClipViewModel : VideoClipViewModel {
        protected float shapeX;
        public float ShapeX {
            get => this.shapeX;
            set => this.RaisePropertyChanged(ref this.shapeX, value);
        }

        protected float shapeY;
        public float ShapeY {
            get => this.shapeY;
            set => this.RaisePropertyChanged(ref this.shapeY, value);
        }

        protected float shapeWidth;
        public float ShapeWidth {
            get => this.shapeWidth;
            set => this.RaisePropertyChanged(ref this.shapeWidth, value);
        }

        protected float shapeHeight;
        public float ShapeHeight {
            get => this.shapeHeight;
            set => this.RaisePropertyChanged(ref this.shapeHeight, value);
        }

        public bool UseScaledRender { get; set; }

        protected UIResizableVideoClipViewModel() {
            this.UseScaledRender = true;
        }

        public void SetShape(float x, float y, float w, float h) {
            this.ShapeX = x;
            this.ShapeY = y;
            this.ShapeWidth = w;
            this.ShapeHeight = h;
        }

        public void TranslateForScaledRender(IViewPort ogl) {
            GL.Translate(this.ShapeX, this.ShapeY, 0f);
            GL.Scale(ogl.Width * (this.ShapeWidth / ogl.Width), ogl.Height * (this.ShapeHeight / ogl.Height), 1f);
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