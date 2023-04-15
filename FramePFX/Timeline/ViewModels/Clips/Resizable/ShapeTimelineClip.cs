using FramePFX.Render;
using FramePFX.ResourceManaging.Items;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Timeline.ViewModels.Clips.Resizable {
    public class ShapeTimelineClip : PositionableTimelineClip {
        private ResourceShapeColour resource;
        public ResourceShapeColour Resource {
            get => this.resource;
            set {
                if (this.resource != null)
                    this.resource.OnResourceModified -= this.OnResourceModified;
                this.RaisePropertyChanged(ref this.resource, value);
                if (value != null)
                    value.OnResourceModified += this.OnResourceModified;
            }
        }

        public float R => this.resource?.Red ?? 0f;

        public float G => this.resource?.Green ?? 0f;

        public float B => this.resource?.Blue ?? 0f;

        public float A => this.resource?.Alpha ?? 0f;

        public ShapeTimelineClip() {
            this.UseScaledRender = true;
        }

        private void OnResourceModified(string propetyname) {
            switch (propetyname) {
                case nameof(this.R):
                case nameof(this.G):
                case nameof(this.B):
                case nameof(this.A):
                    this.RaisePropertyChanged(propetyname);
                    this.InvalidateRender();
                    break;
            }
        }

        // Just gonna put rendering in the ViewModels... sure it's not very "MVVM-ey", but
        // rendering is done off the WPF main thread, so it can't be done in the UI controls
        // because dependency property access must be done on the main thread, and having
        // 3 sets of the same data ("ClipImpl", "ClipViewModel", "ClipControl") is just annoying
        // ViewModel data is thread-safe for the most part, because it references a field

        public override void RenderCore(IViewPort vp, long frame) {
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(this.R, this.G, this.B, this.A);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(1, 0, 0);
            GL.Vertex3(1, 1, 0);
            GL.Vertex3(0, 1, 0);
            GL.End();
        }
    }
}