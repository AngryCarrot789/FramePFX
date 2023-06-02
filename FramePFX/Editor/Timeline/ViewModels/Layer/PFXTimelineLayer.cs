using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Layer;
using FramePFX.Editor.Timeline.Utils;
using FramePFX.Editor.Timeline.ViewModels.Clips;
using FramePFX.Editor.Timeline.ViewModels.Clips.Resizable;
using FramePFX.ResourceManaging;
using FramePFX.ResourceManaging.Items;
using FramePFX.ResourceManaging.ViewModels;

namespace FramePFX.Editor.Timeline.ViewModels.Layer {
    public abstract class PFXTimelineLayer : BaseViewModel, IResourceDropNotifier {
        protected readonly EfficientObservableCollection<PFXClipViewModel> clips;

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
            set => this.RaisePropertyChanged(ref this.layerColour, value);
        }

        public PFXTimeline Timeline { get; }

        public AsyncRelayCommand RenameLayerCommand { get; }

        public ReadOnlyObservableCollection<PFXClipViewModel> Clips { get; }

        public ILayerHandle Control { get; set; }

        protected PFXTimelineLayer(PFXTimeline timeline) {
            this.clips = new EfficientObservableCollection<PFXClipViewModel>();
            this.clips.CollectionChanged += this.ClipsOnCollectionChanged;
            this.Clips = new ReadOnlyObservableCollection<PFXClipViewModel>(this.clips);
            this.Timeline = timeline;
            this.MaxHeight = 200d;
            this.MinHeight = 40;
            this.Height = 60;
            this.layerColour = LayerColours.GetRandomColour();
            this.RenameLayerCommand = new AsyncRelayCommand(async () => {
                string result = await IoC.UserInput.ShowSingleInputDialogAsync("Change layer name", "Input a new layer name:", this.Name ?? "", this.Timeline.LayerNameValidator);
                if (result != null) {
                    this.Name = result;
                }
            });
        }

        public abstract PFXClipViewModel SliceClip(PFXClipViewModel clip, long frame);

        protected virtual void ClipsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.OldItems != null) {
                foreach (object x in e.OldItems) {
                    if (x is PFXClipViewModel clip) {
                        clip.Layer = null;
                    }
                }
            }

            if (e.NewItems != null) {
                foreach (object x in e.NewItems) {
                    if (x is PFXClipViewModel clip) {
                        clip.Layer = this;
                    }
                }
            }
        }

        public PFXShapeClip CreateSquareClip(long begin, long duration, ResourceRGBA colour) {
            PFXShapeClip timelineClip = new PFXShapeClip {
                Resource = colour, FrameBegin = begin, FrameDuration = duration
            };

            this.AddClip(timelineClip);
            return timelineClip;
        }

        public PFXMPEGMediaClip CreateMediaClip(long begin, long duration, ResourceMedia media) {
            //TODO: use proper values once the resource object provides that info
            Resolution resolution = media.GetResolution();
            PFXMPEGMediaClip clip = new PFXMPEGMediaClip {
                Resource = media, FrameBegin = begin, FrameDuration = duration,
                Width = resolution.Width, Height = resolution.Height,
                Header = media.Id
            };

            this.AddClip(clip);
            return clip;
        }

        public virtual void AddClip(PFXClipViewModel clip) {
            this.clips.Add(clip);
        }

        public void MakeTopMost(PFXVideoClipViewModel videoClip) {
            int endIndex = this.Clips.Count - 1;
            int index = this.Clips.IndexOf(videoClip);
            if (index == -1 || index == endIndex) {
                return;
            }

            this.clips.Move(index, endIndex);
        }

        public bool RemoveClip(PFXClipViewModel clip) {
            if (this.clips.Contains(clip)) {
                clip.OnRemoving(this);
                this.clips.Remove(clip); // just in case for some weird reason the clip removes another clip
                return true;
            }

            return false;
        }

        public virtual async Task OnVideoResourceDropped(ResourceItemViewModel resourceItemViewModel, long frameBegin) {
            double fps = this.Timeline.Project.FrameRate;
            long duration = Math.Min((long) Math.Floor(fps * 5), this.Timeline.MaxDuration - frameBegin);
            if (duration <= 0) {
                return;
            }

            if (resourceItemViewModel.TryGetResource(out ResourceItem resource)) {
                if (resource is ResourceRGBA shape) {
                    PFXShapeClip square = this.CreateSquareClip(frameBegin, duration, shape);
                    square.SetShape(0, 0, 200f, 200f);
                    square.Header = resource.Id;
                }
                else if (resource is ResourceMedia media) {
                    media.OpenDecoder();
                    TimeSpan span = media.GetDuration();
                    long dur = (long) Math.Floor(span.TotalSeconds * fps);
                    if (dur < 2) {
                        // image files are 1
                        dur = duration;
                    }

                    if (dur > 0) {
                        this.CreateMediaClip(frameBegin, dur, media);
                    }
                    else {
                        await IoC.MessageDialogs.ShowMessageAsync("Invalid media", "This media has a duration of 0 and cannot be added to the timeline");
                    }
                }
            }
        }

        public IEnumerable<PFXClipViewModel> GetClipsAtFrame(long frame) {
            return this.Clips.Where(clip => clip.IntersectsFrameAt(frame));
        }
    }
}
