using System.Collections.Generic;
using System.Collections.ObjectModel;
using FramePFX.Core;
using FramePFX.Timeline.Layer;
using FramePFX.Timeline.Layer.Clips;

namespace FramePFX.Timeline {
    public class TimelineViewModel : BaseViewModel {
        public static TimelineViewModel Instance { get; set; }

        public ObservableCollection<LayerViewModel> Layers { get; }

        public TimelineControl Control { get; set; }

        public INativePlayHead PlayHead { get; set; }

        private long maxDuration;
        public long MaxDuration {
            get => this.maxDuration;
            set => this.RaisePropertyChanged(ref this.maxDuration, value);
        }

        private long playHeadFrame;
        public long PlayHeadFrame {
            get => this.playHeadFrame;
            set {
                long oldValue = this.playHeadFrame;
                if (oldValue == value) {
                    return;
                }

                this.RaisePropertyChanged(ref this.playHeadFrame, value);
                this.OnPlayHeadMoved(oldValue, value);
            }
        }

        public bool IsRenderDirty { get; set; }

        public TimelineViewModel() {
            this.Layers = new ObservableCollection<LayerViewModel>();
            this.MaxDuration = 10000;
            this.PlayHeadFrame = 0;
            LayerViewModel l1 = this.CreateLayer("Layer 1");
            l1.CreateSquareClip(0, 50).SetShape(5f, 5f, 50f, 50f).Name = "00";
            l1.CreateSquareClip(100, 150).SetShape(55f, 5f, 50f, 50f).Name = "01";
            l1.CreateSquareClip(275, 50).SetShape(110f, 5f, 50f, 50f).Name = "02";

            LayerViewModel l2 = this.CreateLayer("Layer 2");
            l2.CreateSquareClip(0, 100).SetShape(5f, 55f, 50f, 50f).Name = "03";
            l2.CreateSquareClip(100, 50).SetShape(55f, 55f, 50f, 50f).Name = "04";
            l2.CreateSquareClip(175, 75).SetShape(110f, 55f, 50f, 50f).Name = "05";

            Instance = this;
            this.IsRenderDirty = true;
        }

        private void OnPlayHeadMoved(long oldFrame, long frame) {
            this.IsRenderDirty = true;
        }

        public IEnumerable<ClipViewModel> GetClipsIntersectingFrame(long frame) {
            foreach (LayerViewModel layer in this.Layers) {
                foreach (ClipViewModel clip in layer.Clips) {
                    if (clip.IntersectsFrameAt(frame)) {
                        yield return clip;
                    }
                }
            }
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
