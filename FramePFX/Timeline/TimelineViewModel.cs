using System.Collections.ObjectModel;
using FramePFX.Core;
using FramePFX.Timeline.Layer;

namespace FramePFX.Timeline {
    public class TimelineViewModel : BaseViewModel {
        public ObservableCollection<LayerViewModel> Layers { get; }

        public TimelineControl Control { get; set; }

        private long maxDuration;
        public long MaxDuration {
            get => this.maxDuration;
            set => this.RaisePropertyChanged(ref this.maxDuration, value);
        }

        private long playHeadFrame;
        public long PlayHeadFrame {
            get => this.playHeadFrame;
            set => this.RaisePropertyChanged(ref this.playHeadFrame, value);
        }

        public TimelineViewModel() {
            this.Layers = new ObservableCollection<LayerViewModel>();
            this.MaxDuration = 10000;
            LayerViewModel l1 = this.CreateLayer("Layer 1");
            l1.CreateVideoClip(0, 50);
            l1.CreateVideoClip(100, 150);
            l1.CreateVideoClip(275, 50);

            LayerViewModel l2 = this.CreateLayer("Layer 2");
            l2.CreateVideoClip(0, 100);
            l2.CreateVideoClip(100, 50);
            l2.CreateVideoClip(175, 75);
        }

        public LayerViewModel CreateLayer(string name = null) {
            LayerViewModel layer = new LayerViewModel(this) {
                Name = name ?? $"Layer {this.Layers.Count + 1}"
            };
            this.Layers.Add(layer);
            return layer;
        }

        public LayerViewModel GetPrevious(LayerViewModel layer) {
            int index = this.Layers.IndexOf(layer);
            return index < 1 ? null : this.Layers[index - 1];
        }
    }
}
