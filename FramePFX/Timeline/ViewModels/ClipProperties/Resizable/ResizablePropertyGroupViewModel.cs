using FramePFX.Timeline.ViewModels.Clips.Resizable;

namespace FramePFX.Timeline.ViewModels.ClipProperties.Resizable {
    public class ResizablePropertyGroupViewModel : PropertyGroupViewModel {
        public float ShapeX {
            get => this.Clip.ShapeX;
            set {
                this.Clip.ShapeX = value;
                this.OnModified();
            }
        }

        public float ShapeY {
            get => this.Clip.ShapeY;
            set {
                this.Clip.ShapeY = value;
                this.OnModified();
            }
        }

        public float ShapeWidth {
            get => this.Clip.ShapeWidth;
            set {
                this.Clip.ShapeWidth = value;
                this.OnModified();
            }
        }

        public float ShapeHeight {
            get => this.Clip.ShapeHeight;
            set {
                this.Clip.ShapeHeight = value;
                this.OnModified();
            }
        }

        public ResizableVideoClipViewModel Clip { get; }

        public ResizablePropertyGroupViewModel(ResizableVideoClipViewModel clip) : base("Transformation") {
            this.Clip = clip;
        }

        public override void OnModified() {
            base.OnModified();
            this.Clip.Container.MarkForRender();
        }
    }
}