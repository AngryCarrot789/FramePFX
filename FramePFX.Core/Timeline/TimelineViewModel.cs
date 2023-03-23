using System.Collections.Generic;
using System.Collections.ObjectModel;
using FramePFX.Core;
using FramePFX.Core.Timeline;
using FramePFX.Timeline.Layer;
using FramePFX.Timeline.Layer.Clips;

namespace FramePFX.Timeline {
    public class TimelineViewModel : BaseViewModel {
        private readonly ObservableCollection<LayerViewModel> layers;

        private volatile bool isSteppingFrame;
        public volatile bool isFramePropertyChangeScheduled;

        public static TimelineViewModel Instance { get; set; }

        /// <summary>
        /// This timeline's layer collection
        /// </summary>
        public ReadOnlyObservableCollection<LayerViewModel> Layers { get; }

        /// <summary>
        /// A handle to the actual timeline UI control
        /// </summary>
        public ITimelineHandle Handle { get; set; }

        /// <summary>
        /// A handle to the actual play head UI control
        /// </summary>
        public IPlayHeadHandle PlayHeadHandle { get; set; }

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

                if (value >= this.MaxDuration) {
                    value = this.MaxDuration - 1;
                }

                if (this.isSteppingFrame) {
                    this.playHeadFrame = value;
                }
                else {
                    this.RaisePropertyChanged(ref this.playHeadFrame, value);
                    this.OnPlayHeadMoved(oldValue, value);
                }
            }
        }

        public bool IsAtLastFrame => this.PlayHeadFrame >= (this.MaxDuration - 1);

        public bool IsRenderDirty { get; set; }

        public VideoEditorViewModel VideoEditorView { get; }

        public TimelineViewModel(VideoEditorViewModel videoEditorView) {
            this.VideoEditorView = videoEditorView;
            this.layers = new ObservableCollection<LayerViewModel>();
            this.Layers = new ReadOnlyObservableCollection<LayerViewModel>(this.layers);
            this.MaxDuration = 10000;
            this.PlayHeadFrame = 0;
            this.IsRenderDirty = true;
            Instance = this;
        }

        private void OnPlayHeadMoved(long oldFrame, long frame) {
            if (oldFrame != frame) {
                this.IsRenderDirty = true;
            }
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
            this.layers.Add(layer);
            return layer;
        }

        public LayerViewModel GetPrevious(LayerViewModel layer) {
            int index = this.Layers.IndexOf(layer);
            return index < 1 ? null : this.Layers[index - 1];
        }

        public static long WrapIndex(long index, long endIndex) {
            return index >= endIndex ? (index - endIndex) : index;
        }

        /// <summary>
        /// Steps the play head for the next frame
        /// </summary>
        public void StepFrame() {
            this.isSteppingFrame = true;
            long oldFrame = this.playHeadFrame;
            this.playHeadFrame = WrapIndex(this.playHeadFrame + 1, this.maxDuration);
            this.OnPlayHeadMoved(oldFrame, this.playHeadFrame);
            if (!this.isFramePropertyChangeScheduled) {
                this.isFramePropertyChangeScheduled = true;
                IoC.Dispatcher.Invoke(() => {
                    this.RaisePropertyChanged(nameof(this.PlayHeadFrame));
                    this.isFramePropertyChangeScheduled = false;
                });
            }

            this.isSteppingFrame = false;
        }
    }
}
