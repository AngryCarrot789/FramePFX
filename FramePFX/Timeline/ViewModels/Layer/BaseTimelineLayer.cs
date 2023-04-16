using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.UserInputs;
using FramePFX.ResourceManaging;
using FramePFX.ResourceManaging.Items;
using FramePFX.Timeline.Layer;
using FramePFX.Timeline.ViewModels.Clips;
using FramePFX.Timeline.ViewModels.Clips.Resizable;

namespace FramePFX.Timeline.ViewModels.Layer {
    public abstract class BaseTimelineLayer : BaseViewModel, IResourceDropNotifier {
        protected readonly EfficientObservableCollection<BaseTimelineClip> clips;

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

        // Feels so wrong having colours here for some reason... should be done in a converter with enums maybe?
        private string layerColour;
        public string LayerColour {
            get => this.layerColour;
            set => RaisePropertyChanged(ref this.layerColour, value);
        }

        public EditorTimeline Timeline { get; }

        public ICommand RenameLayerCommand { get; }

        public ReadOnlyObservableCollection<BaseTimelineClip> Clips { get; }

        public ILayerHandle Control { get; set; }

        public BaseTimelineLayer(EditorTimeline timeline) {
            this.clips = new EfficientObservableCollection<BaseTimelineClip>();
            this.clips.CollectionChanged += this.ClipsOnCollectionChanged;
            this.Clips = new ReadOnlyObservableCollection<BaseTimelineClip>(this.clips);
            this.Timeline = timeline;
            this.MaxHeight = 200d;
            this.MinHeight = 40;
            this.Height = 60;
            this.layerColour = LayerColours.GetRandomColour();
            this.RenameLayerCommand = new RelayCommand(() => {
                string result = CoreIoC.UserInput.ShowSingleInputDialog("Change layer name", "Input a new layer name:", this.Name ?? "", this.Timeline.LayerNameValidator);
                if (result != null) {
                    this.Name = result;
                }
            });
        }

        public abstract BaseTimelineClip SliceClip(BaseTimelineClip clip, long frame);

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
                    if (x is BaseTimelineClip clip) {
                        clip.Layer = this;
                    }
                }
            }
        }

        public ShapeTimelineClip CreateSquareClip(long begin, long duration, ResourceShapeColour colour) {
            ShapeTimelineClip timelineClip = new ShapeTimelineClip {
                Resource = colour, FrameBegin = begin, FrameDuration = duration
            };

            this.AddClip(timelineClip);
            return timelineClip;
        }

        public VideoMediaTimelineClip CreateMediaClip(long begin, long duration, ResourceVideoMedia media) {
            //TODO: use proper values once the resource object provides that info
            VideoMediaTimelineClip clip = new VideoMediaTimelineClip {
                Resource = media, FrameBegin = begin, FrameDuration = duration,
                Width = 1280, Height = 720,
                Name = media.UniqueID
            };

            this.AddClip(clip);
            return clip;
        }

        public virtual void AddClip(BaseTimelineClip clip) {
            this.clips.Add(clip);
        }

        public void MakeTopMost(TimelineVideoClip videoClip) {
            int endIndex = this.Clips.Count - 1;
            int index = this.Clips.IndexOf(videoClip);
            if (index == -1 || index == endIndex) {
                return;
            }

            this.clips.Move(index, endIndex);
        }

        public bool RemoveClip(BaseTimelineClip clip) {
            if (this.clips.Contains(clip)) {
                clip.OnRemovingCore(this);
                this.clips.Remove(clip); // just in case for some weird reason the clip removes another clip
                return true;
            }

            return false;
        }

        public virtual Task OnVideoResourceDropped(ResourceItem resource, long frameBegin) {
            long duration = Math.Min(300, this.Timeline.MaxDuration - frameBegin);

            if (duration <= 0) {
                return Task.CompletedTask;
            }

            if (resource is ResourceShapeColour shape) {
                ShapeTimelineClip square = this.CreateSquareClip(frameBegin, duration, shape);
                square.SetShape(0, 0, 200f, 200f);
                square.Name = resource.UniqueID;
            }
            else if (resource is ResourceVideoMedia media) {
                this.CreateMediaClip(frameBegin, duration, media);
            }

            return Task.CompletedTask;
        }
    }
}
