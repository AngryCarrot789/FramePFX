using FramePFX.Core;
using FramePFX.Render;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Timeline.Layer.Clips {
    public class SquareClipViewModel : VideoClipViewModel {
        private float posX;
        public float PosX {
            get => this.posX;
            set => this.RaisePropertyChanged(ref this.posX, value);
        }

        private float posY;
        public float PosY {
            get => this.posY;
            set => this.RaisePropertyChanged(ref this.posY, value);
        }

        private float width;
        public float Width {
            get => this.width;
            set => this.RaisePropertyChanged(ref this.width, value);
        }

        private float height;
        public float Height {
            get => this.height;
            set => this.RaisePropertyChanged(ref this.height, value);
        }

        public SquareClipViewModel(LayerViewModel layer) : base(layer) {
            this.PosX = 10f;
            this.PosY = 10f;
            this.Width = 250f;
            this.PosX = 100f;
        }

        public SquareClipViewModel SetShape(float x, float y, float w, float h) {
            this.PosX = x;
            this.PosY = y;
            this.Width = w;
            this.Height = h;
            return this;
        }

        public override void Render() {
            GL.PushMatrix();

            OGLContext ogl = IoC.Instance.Provide<OGLContext>();
            GL.Translate(this.posX, this.posY, 0f);
            GL.Scale(ogl.Width * (this.Width / ogl.Width), ogl.Height * (this.Height / ogl.Height), 1f);
            // GL.Rotate(this.rotZ, 0, 0, 1);

            GL.Begin(PrimitiveType.Quads);
            GL.Color3(0.1f, 0.1f, 0.4f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(1, 0, 0);
            GL.Vertex3(1, 1, 0);
            GL.Vertex3(0, 1, 0);
            GL.End();

            GL.PopMatrix();
        }
    }
}