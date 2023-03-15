using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Views.Dialogs.UserInputs;
using FramePFX.Timeline.Layer.Clips;

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

        public TimelineViewModel Timeline { get; }

        public ICommand RenameLayerCommand { get; }

        public ObservableCollection<ClipViewModel> Items { get; }

        public TimelineLayerControl Control { get; set; }

        public LayerViewModel(TimelineViewModel timeline) {
            this.Items = new ObservableCollection<ClipViewModel>();
            this.Timeline = timeline;
            this.MaxHeight = 200d;
            this.MinHeight = 40;
            this.Height = 60;

            this.RenameLayerCommand = new RelayCommand(() => {
                string result = IoC.UserInput.ShowSingleInputDialog("Change layer name", "Input a new layer name:", this.Name ?? "", new InputValidator((x) => this.Timeline.Layers.Any(b => b.Name == x), "Layer already exists with that name"));
                if (result != null) {
                    this.Name = result;
                }
            });

        }

        public VideoClipViewModel CloneVideoClip(VideoClipViewModel clip) {
            return this.CreateVideoClip(clip.FrameBegin, clip.FrameDuration);
        }

        public VideoClipViewModel CreateVideoClip(long begin, long duration) {
            VideoClipViewModel clip = new VideoClipViewModel(this) {
                FrameBegin = begin,
                FrameDuration = duration
            };

            this.Items.Add(clip);
            return clip;
        }

        public void MakeTopMost(ClipViewModel clip) {
            int endIndex = this.Items.Count - 1;
            int index = this.Items.IndexOf(clip);
            if (index == -1 || index == endIndex) {
                return;
            }

            this.Items.Move(index, endIndex);
        }
    }
}
