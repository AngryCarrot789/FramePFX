using FramePFX.Render;
using FramePFX.ResourceManaging.Items;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Editor.Timeline.ViewModels.Clips.Resizable {
    public class PFXShapeClip : PFXAdjustableVideoClip {
        private ResourceRGBA resource;
        public ResourceRGBA Resource {
            get => this.resource;
            set {
                this.RaisePropertyChanged(ref this.resource, value);
                this.RaisePropertyChanged(nameof(this.R));
                this.RaisePropertyChanged(nameof(this.G));
                this.RaisePropertyChanged(nameof(this.B));
                this.RaisePropertyChanged(nameof(this.A));
                this.InvalidateRenderForPropertyChanged();
            }
        }

        public float R => this.resource?.R ?? 0f;

        public float G => this.resource?.G ?? 0f;

        public float B => this.resource?.B ?? 0f;

        public float A => this.resource?.A ?? 1f;

        public PFXShapeClip() {
            this.UseScaledRender = true;
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

        public override PFXBaseClip NewInstanceOverride() {
            return new PFXShapeClip();
        }

        public override void LoadDataIntoClone(PFXBaseClip clone) {
            base.LoadDataIntoClone(clone);
            if (clone is PFXShapeClip clip) {
                clip.resource = this.resource;
            }
        }
    }
}