using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.Project;
using FramePFX.Timeline.Layer;
using FramePFX.Timeline.Layer.Clips;
using FramePFX.Timeline.ViewModels.ClipProperties;
using FramePFX.Timeline.ViewModels.Clips;
using FramePFX.Timeline.ViewModels.Layer;

namespace FramePFX.Timeline {
    public class TimelineViewModel : BaseViewModel {
        private readonly RapidDispatchCallback renderDispatch;
        private readonly ObservableCollection<LayerViewModel> layers;
        private volatile bool ignorePlayHeadPropertyChange;
        public volatile bool isFramePropertyChangeScheduled;

        public ProjectViewModel Project { get; }

        public PlaybackViewportViewModel PlaybackViewport => this.Project.VideoEditor.PlaybackView;

        /// <summary>
        /// This timeline's layer collection
        /// </summary>
        public ReadOnlyObservableCollection<LayerViewModel> Layers { get; }

        private LayerViewModel selectedLayer;
        public LayerViewModel SelectedLayer {
            get => this.selectedLayer;
            set => this.RaisePropertyChanged(ref this.selectedLayer, value);
        }

        public ObservableCollection<BaseClipPropertyItemViewModel> GeneratedProperties { get; }

        private ClipViewModel mainSelectedClip;
        public ClipViewModel MainSelectedClip {
            get => this.mainSelectedClip;
            set => this.RaisePropertyChanged(ref this.mainSelectedClip, value);
        }

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
            set => this.RaisePropertyChangedIfChanged(ref this.maxDuration, value);
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

                if (value < 0) {
                    value = 0;
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

        public RelayCommandParam<string> DeleteSelectedClipsCommand { get; }
        public ICommand DeleteSelectedLayerCommand { get; }

        public TimelineViewModel(ProjectViewModel project) {
            this.Project = project;
            this.layers = new ObservableCollection<LayerViewModel>();
            this.Layers = new ReadOnlyObservableCollection<LayerViewModel>(this.layers);
            this.GeneratedProperties = new ObservableCollection<BaseClipPropertyItemViewModel>();
            this.renderDispatch = new RapidDispatchCallback(this.RenderViewPortAndMarkNotDirty) {
                InvokeLater = true
            };
            this.DeleteSelectedClipsCommand = new RelayCommandParam<string>(async (x) => await this.DeleteSelectedClipsAction(x));
            this.DeleteSelectedLayerCommand = new RelayCommand(this.DeleteSelectedLayerAction);
        }

        public void DeleteSelectedLayerAction() {
            if (this.selectedLayer != null) {
                this.layers.Remove(this.selectedLayer);
            }
        }

        public async Task DeleteSelectedClipsAction(string skipDialog) {
            List<ClipContainerViewModel> list = this.Handle.GetSelectedClips().ToList();
            switch (list.Count) {
                case 0: return;
                case 1:
                    list[0].Layer.DeleteClip(list[0]);
                    return;
                default: {
                    if (skipDialog == "true" || await CoreIoC.MessageDialogs.ShowYesNoDialogAsync("Delete clips", "Do you want to delete these " + list.Count + " clips?")) {
                        foreach (ClipContainerViewModel clip in list) {
                            clip.Layer.DeleteClip(clip);
                        }
                    }

                    return;
                }
            }
        }

        public bool CanRender() {
            return this.Project.VideoEditor.PlaybackView.IsReadyForRender();
        }

        public void MarkRenderDirty() {
            this.IsRenderDirty = true;
            if (this.CanRender()) {
                this.ScheduleRender(false);
            }
        }

        public void OnPlayHeadMoved(long oldFrame, long frame, bool render) {
            if (oldFrame == frame) {
                return;
            }

            if (render && this.CanRender()) {
                this.RenderViewPortAndMarkNotDirty();
            }
            else {
                this.IsRenderDirty = true;
            }
        }

        public void ScheduleRender(bool useCurrentThread = true) {
            if (useCurrentThread) {
                this.RenderViewPortAndMarkNotDirty();
            }
            else {
                this.renderDispatch.Invoke();
            }
        }

        private void RenderViewPortAndMarkNotDirty() {
            this.IsRenderDirty = false;
            IoC.VideoEditor.PlaybackView.RenderTimeline(this);
        }

        public IEnumerable<ClipContainerViewModel> GetClipsOnPlayHead() {
            return this.GetClipsIntersectingFrame(this.playHeadFrame);
        }

        public IEnumerable<ClipContainerViewModel> GetClipsIntersectingFrame(long frame) {
            foreach (LayerViewModel layer in this.layers) {
                foreach (ClipContainerViewModel clip in layer.Clips) {
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
                CoreIoC.Dispatcher.Invoke(() => {
                    this.RaisePropertyChanged(nameof(this.PlayHeadFrame));
                    this.isFramePropertyChangeScheduled = false;
                });
            }

            this.ignorePlayHeadPropertyChange = false;
        }
    }
}
