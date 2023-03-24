using System.Windows;
using System.Windows.Media;
using FramePFX.Controls;
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

        private OGLViewPortControl PART_ViewPort;

        public IColourData ShapeColour {
            get => (IColourData) this.GetValue(ShapeColourProperty);
            set => this.SetValue(ShapeColourProperty, value);
        }

        private bool isRendering;

        public SquareClipControl() {

        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.PART_ViewPort = this.GetTemplateChild("PART_ViewPort") as OGLViewPortControl;
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

        public override void Render(IOGLViewPort ogl, long frame) {
            base.Render(ogl, frame);
            if (this.PART_ViewPort != null) {
                base.Render(this.PART_ViewPort, frame);
                this.PART_ViewPort.FlushFrame();
            }
        }
    }
}