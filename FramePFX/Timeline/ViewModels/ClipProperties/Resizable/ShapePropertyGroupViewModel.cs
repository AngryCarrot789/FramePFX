using FramePFX.ResourceManaging.Items;
using FramePFX.Timeline.ViewModels.Clips.Resizable;

namespace FramePFX.Timeline.ViewModels.ClipProperties.Resizable {
    public class ShapePropertyGroupViewModel : PropertyGroupViewModel {
        public ResourceColourViewModel Resource {
            get => this.Clip.Resource;
            set => this.Clip.Resource = value;
        }

        public float R {
            get => this.Clip.R;
            set => this.Clip.R = value;
        }

        public float G {
            get => this.Clip.G;
            set => this.Clip.G = value;
        }

        public float B {
            get => this.Clip.B;
            set => this.Clip.B = value;
        }

        public float A {
            get => this.Clip.A;
            set => this.Clip.A = value;
        }

        public ShapeClipViewModel Clip { get; }

        public ShapePropertyGroupViewModel(ShapeClipViewModel clip) : base("Shape Data") {
            this.Clip = clip;
        }
    }
}