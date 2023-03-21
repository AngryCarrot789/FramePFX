using FramePFX.Render;
using FramePFX.Timeline.Layer.Clips.Resizable;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Timeline.Layer.Clips {
    public class SquareClipViewModel : ResizableVideoClipViewModel {
        public SquareClipViewModel(LayerViewModel layer) : base(layer) {
            this.PosX = 10f;
            this.PosY = 10f;
            this.Width = 250f;
            this.PosX = 100f;
        }

        public override void RenderCore(OGLContext context) {
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(0.1f, 0.1f, 0.4f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(1, 0, 0);
            GL.Vertex3(1, 1, 0);
            GL.Vertex3(0, 1, 0);
            GL.End();
        }
    }
}