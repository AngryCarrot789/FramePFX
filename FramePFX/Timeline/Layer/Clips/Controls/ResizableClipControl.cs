using System.Windows;
using FramePFX.Render;
using FramePFX.Timeline.Layer.Clips.Data;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Timeline.Layer.Clips.Controls {
    public abstract class ResizableClipControl : BaseClipControl, IClipHandle, IClipRenderTarget {
        private volatile IResizableClipData resizeData;

        public static readonly DependencyProperty ResizeDataProperty =
            DependencyProperty.Register(
                "ResizeData",
                typeof(IResizableClipData),
                typeof(ResizableClipControl),
                new PropertyMetadata(null, (d,e) => ((ResizableClipControl) d).resizeData = (IResizableClipData) e.NewValue));

        public IResizableClipData ResizeData {
            get => (IResizableClipData) this.GetValue(ResizeDataProperty);
            set => this.SetValue(ResizeDataProperty, value);
        }

        public TimelineClipControl ParentClip => this.Parent as TimelineClipControl;

        protected ResizableClipControl() {

        }

        public void TranslateForScaledRender(IOGLViewPort ogl) {
            GL.Translate(this.resizeData.ShapeX, this.resizeData.ShapeY, 0f);
            GL.Scale(ogl.ViewportWidth * (this.resizeData.ShapeWidth / ogl.ViewportWidth), ogl.ViewportHeight * (this.resizeData.ShapeHeight / ogl.ViewportHeight), 1f);
            // GL.Rotate(this.rotZ, 0, 0, 1);
        }

        public void Render(IOGLViewPort ogl, long frame) {
            GL.PushMatrix();
            if (this.resizeData.UseScaledRender) {
                this.TranslateForScaledRender(ogl);
            }

            this.RenderCore(ogl, frame);
            GL.PopMatrix();
        }

        public abstract void RenderCore(IOGLViewPort ogl, long frame);
    }
}