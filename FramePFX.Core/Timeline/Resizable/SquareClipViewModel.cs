using FramePFX.Core.ResourceManaging.Items;
using FramePFX.Render;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Timeline.Layer.Clips.Resizable {
    public class SquareClipViewModel : ResizableVideoClipViewModel {
        private ResourceSquareViewModel resource;
        public ResourceSquareViewModel Resource {
            get => this.resource;
            set => this.RaisePropertyChanged(ref this.resource, value);
        }

        public SquareClipViewModel(LayerViewModel layer) {
            this.Layer = layer;
        }

        public override void RenderCore(IOGLViewPort context, long frame) {
            GL.Begin(PrimitiveType.Quads);
            if (this.Resource != null) {
                GL.Color4(this.resource.Red, this.resource.Green, this.resource.Blue, this.resource.Alpha * this.Layer.Opacity);
            }
            else {
                GL.Color4(1f, 1f, 1f, this.Layer.Opacity);
            }

            GL.Vertex3(0, 0, 0);
            GL.Vertex3(1, 0, 0);
            GL.Vertex3(1, 1, 0);
            GL.Vertex3(0, 1, 0);
            GL.End();
        }
    }
}