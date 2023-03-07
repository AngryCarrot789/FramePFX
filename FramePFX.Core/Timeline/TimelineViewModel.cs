using System.Collections.ObjectModel;
using FrameControl.Core;
using FramePFX.Core.Timeline.Layer;

namespace FramePFX.Core.Timeline {
    public class TimelineViewModel : BaseViewModel {
        public ObservableCollection<LayerViewModel> Layers { get; }

        public TimelineViewModel() {
            this.Layers = new ObservableCollection<LayerViewModel> {
                new VideoLayerViewModel(this) { Name = "Layer 1" },
                new VideoLayerViewModel(this) { Name = "Layer 2" }
            };
        }

        public LayerViewModel GetPrevious(LayerViewModel layer) {
            int index = this.Layers.IndexOf(layer);
            return index < 1 ? null : this.Layers[index - 1];
        }
    }
}
