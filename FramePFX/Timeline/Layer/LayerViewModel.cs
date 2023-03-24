using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.UserInputs;
using FramePFX.ResourceManaging.Items;
using FramePFX.Timeline.Layer.Clips;
using FramePFX.Timeline.Layer.Clips.Resizable;

namespace FramePFX.Timeline.Layer {
    public class LayerViewModel : BaseViewModel {
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

        private float opacity;

        /// <summary>
        /// The opacity of this layer. Between 0f and 1f (not yet implemented properly)
        /// </summary>
        public float Opacity {
            get => this.opacity;
            set => this.RaisePropertyChanged(ref this.opacity, Maths.Clamp(value, 0f, 1f), () => this.Timeline.MarkRenderDirty());
        }

        public TimelineViewModel Timeline { get; }

        public ICommand RenameLayerCommand { get; }

        public ObservableCollection<ClipContainerViewModel> Clips { get; }

        public ILayerHandle Control { get; set; }

        public LayerViewModel(TimelineViewModel timeline) {
            this.Clips = new ObservableCollection<ClipContainerViewModel>();
            this.Clips.CollectionChanged += this.ClipsOnCollectionChanged;
            this.Timeline = timeline;
            this.MaxHeight = 200d;
            this.MinHeight = 40;
            this.Height = 60;
            this.Opacity = 1f;

            this.RenameLayerCommand = new RelayCommand(() => {
                string result = CoreIoC.UserInput.ShowSingleInputDialog("Change layer name", "Input a new layer name:", this.Name ?? "", InputValidator.SingleError(x => this.Timeline.Layers.Any(b => b.Name == x), "Layer already exists with that name"));
                if (result != null) {
                    this.Name = result;
                }
            });
        }

        private void ClipsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (object x in e.NewItems) {
                    if (x is ClipContainerViewModel clip) {
                        clip.Layer = this;
                    }
                }
            }
        }

        public VideoClipContainerViewModel CreateVideoClipContainer(long frameBegin, long frameDuration) {
            return new VideoClipContainerViewModel() {
                Layer = this,
                FrameBegin = frameBegin,
                FrameDuration = frameDuration
            };
        }

        public ShapeClipViewModel CreateSquareClip(long begin, long duration, ResourceColourViewModel colour) {
            VideoClipContainerViewModel container = this.CreateVideoClipContainer(begin, duration);
            ShapeClipViewModel clip = new ShapeClipViewModel {
                Resource = colour
            };

            ClipContainerViewModel.SetClipContent(container, clip);
            this.Clips.Add(container);
            return clip;
        }

        public void MakeTopMost(ClipContainerViewModel clip) {
            int endIndex = this.Clips.Count - 1;
            int index = this.Clips.IndexOf(clip);
            if (index == -1 || index == endIndex) {
                return;
            }

            this.Clips.Move(index, endIndex);
        }

        public void RemoveRegion(long frameBegin, long frameEnd, Action<ClipContainerViewModel> onModified, Action<ClipContainerViewModel> onRemoved) {
            foreach (ClipContainerViewModel clip in this.Clips) {

            }
        }
    }
}
