using FramePFX.Render;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Timeline.Layer.Clips.Resizable {
    public abstract class ResizableVideoClipViewModel : VideoClipViewModel {
        protected float posX;
        public float PosX {
            get => this.posX;
            set => this.RaisePropertyChanged(ref this.posX, value);
        }

        protected float posY;
        public float PosY {
            get => this.posY;
            set => this.RaisePropertyChanged(ref this.posY, value);
        }

        protected float width;
        public float Width {
            get => this.width;
            set => this.RaisePropertyChanged(ref this.width, value);
        }

        protected float height;
        public float Height {
            get => this.height;
            set => this.RaisePropertyChanged(ref this.height, value);
        }

        protected float rotZ;
        public float RotZ {
            get => this.rotZ;
            set => this.RaisePropertyChanged(ref this.rotZ, value);
        }

        /// <summary>
        /// Whether to scale the rendering between 0 and 1 in the plane axis, based on this resizable clip's X, Y, Width and Height properties.
        /// <para>
        /// When false, the rendering must be done based on the OGL view port size, and manually positioned
        /// </para>
        /// </summary>
        public bool UseScaledRender { get; set; }

        public ResizableVideoClipViewModel() {
            this.UseScaledRender = true;
        }

        public void TranslateForScaledRender(IOGLViewPort ogl) {
            GL.Translate(this.posX, this.posY, 0f);
            GL.Scale(ogl.ViewportWidth * (this.Width / ogl.ViewportWidth), ogl.ViewportHeight * (this.Height / ogl.ViewportHeight), 1f);
            GL.Rotate(this.rotZ, 0, 0, 1);
        }

        public ResizableVideoClipViewModel SetShape(float x, float y, float w, float h) {
            this.PosX = x;
            this.PosY = y;
            this.Width = w;
            this.Height = h;
            return this;
        }

        public sealed override void Render(IOGLViewPort ogl, long frame) {
            GL.PushMatrix();
            if (this.UseScaledRender) {
                this.TranslateForScaledRender(ogl);
            }

            this.RenderCore(ogl, frame);
            GL.PopMatrix();
        }

        public abstract void RenderCore(IOGLViewPort context, long frame);
    }
}