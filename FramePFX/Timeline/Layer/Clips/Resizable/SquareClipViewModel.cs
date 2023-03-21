using FramePFX.Render;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Timeline.Layer.Clips.Resizable {
    public class SquareClipViewModel : ResizableVideoClipViewModel {
        private float r;
        private float g;
        private float b;
        private float a;

        public float Red { get => this.r; set => this.RaisePropertyChanged(ref this.r, value); }

        public float Green { get => this.g; set => this.RaisePropertyChanged(ref this.g, value); }

        public float Blue { get => this.b; set => this.RaisePropertyChanged(ref this.b, value); }

        public float Alpha { get => this.a; set => this.RaisePropertyChanged(ref this.a, value); }

        public SquareClipViewModel(LayerViewModel layer) {
            this.Layer = layer;
            this.Red = this.Green = this.Blue = 1f;
            this.Alpha = 1f;
        }

        public override void RenderCore(OGLViewPortContext context, long frame) {
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(this.r, this.g, this.b, this.a * this.Layer.Opacity);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(1, 0, 0);
            GL.Vertex3(1, 1, 0);
            GL.Vertex3(0, 1, 0);
            GL.End();
        }
    }
}