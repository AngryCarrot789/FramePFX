using FrameControl.Core;

namespace FramePFX.Core.Timeline.Layer.Clips {
    public class ClipViewModel : BaseViewModel {
        public LayerViewModel Layer { get; }

        public ClipViewModel(LayerViewModel layer) {
            this.Layer = layer;
        }
    }
}
