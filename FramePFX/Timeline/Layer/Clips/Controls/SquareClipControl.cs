using System.Windows;
using FramePFX.Render;
using FramePFX.Timeline.Layer.Clips.Data;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Timeline.Layer.Clips.Controls {
    public class SquareClipControl : ResizableClipControl {
        private volatile IColourData colourData;

        public static readonly DependencyProperty ShapeColourProperty =
            DependencyProperty.Register(
                "ShapeColour",
                typeof(IColourData),
                typeof(SquareClipControl),
                new PropertyMetadata((d,e) => ((SquareClipControl) d).colourData = (IColourData) e.NewValue));

        public IColourData ShapeColour {
            get => (IColourData) this.GetValue(ShapeColourProperty);
            set => this.SetValue(ShapeColourProperty, value);
        }

        public SquareClipControl() {

        }

        public override void RenderCore(IOGLViewPort ogl, long frame) {
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(this.colourData.R, this.colourData.G, this.colourData.B, this.colourData.A);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(1, 0, 0);
            GL.Vertex3(1, 1, 0);
            GL.Vertex3(0, 1, 0);
            GL.End();
        }
    }
}