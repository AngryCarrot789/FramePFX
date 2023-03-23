using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FramePFX.Core.Render;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Timeline {
    public class TimelineViewModel : BaseViewModel {
        private readonly ObservableCollection<LayerViewModel> layers;

        private volatile bool ignorePlayHeadPropertyChange;
        public volatile bool isFramePropertyChangeScheduled;

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

                if (this.ignorePlayHeadPropertyChange) {
                    this.playHeadFrame = value;
                }
                else {
                    this.RaisePropertyChanged(ref this.playHeadFrame, value);
                    this.OnPlayHeadMoved(oldValue, value, true);
                }
            }
        }

        public bool IsAtLastFrame => this.PlayHeadFrame >= (this.MaxDuration - 1);

        public bool IsRenderDirty { get; private set; }

        private readonly RapidDispatchCallback renderDispatch = new RapidDispatchCallback();

        public TimelineViewModel() {
            this.layers = new ObservableCollection<LayerViewModel>();
            this.Layers = new ReadOnlyObservableCollection<LayerViewModel>(this.layers);
            this.MaxDuration = 10000;
            this.PlayHeadFrame = 0;
            this.MarkRenderDirty();
        }

        public bool CanRender() {
            return IoC.Editor?.MainViewPort?.IsReadyForRender ?? false;
        }

        public void MarkRenderDirty() {
            this.IsRenderDirty = true;
            if (this.CanRender()) {
                this.ScheduleRender(true);
            }
        }

        public void OnPlayHeadMoved(long oldFrame, long frame, bool render) {
            if (oldFrame == frame) {
                return;
            }

            if (render && this.CanRender()) {
                this.RenderViewPort();
            }
            else {
                this.IsRenderDirty = true;
            }
        }

        public void ScheduleRender(bool useCurrentThread = true) {
            if (useCurrentThread) {
                this.RenderViewPort();
            }
            else {
                this.renderDispatch.Invoke(this.RenderViewPort);
            }
        }

        private void RenderViewPort() {
            this.IsRenderDirty = false;
            IoC.Editor.RenderViewPort();
        }

        public IEnumerable<ClipViewModel> GetClipsOnPlayHead() {
            return this.GetClipsIntersectingFrame(this.playHeadFrame);
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
            // only works properly if index is less than (endIndex * 2)
            // e.g. if index is 2005 and endIndex is 1000, this function will return 1005, not 5
            // Assume that will never be the case though...
            return index >= endIndex ? (index - endIndex) : index;
        }

        /// <summary>
        /// Steps the play head for the next frame (typically from the playback thread)
        /// </summary>
        public void StepFrame(long change = 1L) {
            this.ignorePlayHeadPropertyChange = true;
            long duration = this.maxDuration;
            long oldFrame = this.playHeadFrame;
            // Clamp between 0 and max duration. also clamp change in safe duration range
            long newFrame = Math.Max(this.playHeadFrame + Maths.Clamp(change, -duration, duration), 0);
            this.playHeadFrame = WrapIndex(newFrame, duration);
            this.OnPlayHeadMoved(oldFrame, this.playHeadFrame, false);
            if (!this.isFramePropertyChangeScheduled) {
                this.isFramePropertyChangeScheduled = true;
                IoC.Dispatcher.Invoke(() => {
                    this.RaisePropertyChanged(nameof(this.PlayHeadFrame));
                    this.isFramePropertyChangeScheduled = false;
                });
            }

            this.ignorePlayHeadPropertyChange = false;
        }
    }
}
