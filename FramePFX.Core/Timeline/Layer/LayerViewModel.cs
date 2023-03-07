using System;
using System.Collections.ObjectModel;
using FrameControl.Core;
using FramePFX.Core.Timeline.Layer.Clips;

namespace FramePFX.Core.Timeline.Layer {
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

        public ObservableCollection<ClipViewModel> Clips { get; }

        public TimelineViewModel Timeline { get; }

        public LayerViewModel(TimelineViewModel timeline) {
            this.Timeline = timeline;
            this.MaxHeight = 200d;
            this.MinHeight = 40;
            this.Height = 60;
            this.Clips = new ObservableCollection<ClipViewModel>();
            this.Clips.Add(new ClipViewModel(this));
        }

        public bool SetHeight(double value) {
            if (value < this.MinHeight || value > this.MaxHeight) {
                return false;
            }

            this.Height = value;
            return true;
        }
    }
}
