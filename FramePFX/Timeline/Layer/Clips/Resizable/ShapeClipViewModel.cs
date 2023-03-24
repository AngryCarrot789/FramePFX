using FramePFX.ResourceManaging.Items;
using FramePFX.Timeline.Layer.Clips.Data;

namespace FramePFX.Timeline.Layer.Clips.Resizable {
    public class ShapeClipViewModel : UIResizableVideoClipViewModel, IColourData {
        private ResourceColourViewModel resource;
        public ResourceColourViewModel Resource {
            get => this.resource;
            set => this.RaisePropertyChanged(ref this.resource, value);
        }

        public float R {
            get => this.resource.Red;
            set => this.resource.Red = value;
        }

        public float G {
            get => this.resource.Green;
            set => this.resource.Green = value;
        }

        public float B {
            get => this.resource.Blue;
            set => this.resource.Blue = value;
        }

        public float A {
            get => this.resource.Alpha;
            set => this.resource.Alpha = value;
        }

        public ShapeClipViewModel() {
            this.Resource = new ResourceColourViewModel() {
                Red = 1f,
                Green = 1f,
                Blue = 1f,
                Alpha = 1f
            };
        }
    }
}