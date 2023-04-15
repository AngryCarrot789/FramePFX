using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.UserInputs;
using FramePFX.ResourceManaging;
using FramePFX.ResourceManaging.Items;
using FramePFX.Timeline.Layer;
using FramePFX.Timeline.ViewModels.Clips;
using FramePFX.Timeline.ViewModels.Clips.Resizable;

namespace FramePFX.Timeline.ViewModels.Layer {
    public abstract class TimelineLayer : BaseViewModel, IResourceDropNotifier {
        private EfficientObservableCollection<BaseTimelineClip> clips;

        private string name;
        public string Name {
            get => this.name;
            set => this.RaisePropertyChanged(ref this.name, value);
        }

        private double minHeight;
        public double MinHeight {
            get => this.minHeight;
            set => this.RaisePropertyChanged(ref this.minHeight, value);
        }

        private double maxHeight;
        public double MaxHeight {
            get => this.maxHeight;
            set => this.RaisePropertyChanged(ref this.maxHeight, value);
        }

        private double height;
        public double Height {
            get => this.height;
            set => this.RaisePropertyChanged(ref this.height, Math.Max(Math.Min(value, this.MaxHeight), this.MinHeight));
        }

        public EditorTimeline Timeline { get; }

        public ICommand RenameLayerCommand { get; }

        public ReadOnlyObservableCollection<BaseTimelineClip> Clips { get; }

        public ILayerHandle Control { get; set; }

        public TimelineLayer(EditorTimeline timeline) {
            this.clips = new EfficientObservableCollection<BaseTimelineClip>();
            this.clips.CollectionChanged += this.ClipsOnCollectionChanged;
            this.Clips = new ReadOnlyObservableCollection<BaseTimelineClip>(this.clips);
            this.Timeline = timeline;
            this.MaxHeight = 200d;
            this.MinHeight = 40;
            this.Height = 60;

            this.RenameLayerCommand = new RelayCommand(() => {
                string result = CoreIoC.UserInput.ShowSingleInputDialog("Change layer name", "Input a new layer name:", this.Name ?? "", InputValidator.SingleError(x => this.Timeline.Layers.Any(b => b.Name == x), "Layer already exists with that name"));
                if (result != null) {
                    this.Name = result;
                }
            });
        }

        protected virtual void ClipsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.OldItems != null) {
                foreach (object x in e.OldItems) {
                    if (x is BaseTimelineClip clip) {
                        clip.Layer = null;
                    }
                }
            }

            if (e.NewItems != null) {
                foreach (object x in e.NewItems) {
                    if (x is TimelineVideoClip clip) {
                        clip.Layer = this;
                    }
                }
            }
        }

        public ShapeTimelineClip CreateSquareClip(long begin, long duration, ResourceShapeColour colour) {
            ShapeTimelineClip timelineClip = new ShapeTimelineClip {
                Resource = colour, FrameBegin = begin, FrameDuration = duration
            };

            this.clips.Add(timelineClip);
            return timelineClip;
        }

        // public ImageClipViewModel CreateImageClip(long begin, long duration, ImageResourceViewModel image) {
        //     ClipContainerViewModel container = this.CreateVideoClipContainer(begin, duration);
        //     ClipViewModel clip = new ClipViewModel {
        //         Resource = image
        //     };
        //     ClipContainerViewModel.SetClipContent(container, clip);
        //     ClipViewModel.SetContainer(clip, container);
        //     this.Clips.Add(container);
        //     return clip;
        // }

        public void MakeTopMost(TimelineVideoClip videoClip) {
            int endIndex = this.Clips.Count - 1;
            int index = this.Clips.IndexOf(videoClip);
            if (index == -1 || index == endIndex) {
                return;
            }

            this.clips.Move(index, endIndex);
        }

        public void RemoveRegion(long frameBegin, long frameEnd, Action<TimelineVideoClip> onModified, Action<TimelineVideoClip> onRemoved) {
        }

        public bool DeleteClip(BaseTimelineClip clip) {
            return this.clips.Remove(clip);
        }

        public virtual Task OnVideoResourceDropped(ResourceItem resource, long frameBegin) {
            if (resource is ResourceShapeColour shape) {
                long endIndex = frameBegin + 300;
                if (endIndex >= this.Timeline.MaxDuration) {
                    endIndex = this.Timeline.MaxDuration - 1;
                }

                if (endIndex > frameBegin) {
                    ShapeTimelineClip square = this.CreateSquareClip(frameBegin, endIndex - frameBegin, shape);
                    square.SetShape(0, 0, 200f, 200f);
                    square.Name = resource.UniqueID;
                }
            }
            else if (resource is ResourceImage imageResource) {
                // this.CreateImageClip(0, 300, imageResource);
            }

            return Task.CompletedTask;
        }
    }
}
