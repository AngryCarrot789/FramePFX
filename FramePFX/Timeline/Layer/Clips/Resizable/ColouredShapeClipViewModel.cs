using FramePFX.ResourceManaging.Items;

namespace FramePFX.Timeline.Layer.Clips.Resizable {
    public class SquareClipViewModel : ResizableVideoClipViewModel {
        private ResourceColourViewModel resource;
        public ResourceColourViewModel Resource {
            get => this.resource;
            set => this.RaisePropertyChanged(ref this.resource, value);
        }

        public SquareClipViewModel() {

        }
    }
}