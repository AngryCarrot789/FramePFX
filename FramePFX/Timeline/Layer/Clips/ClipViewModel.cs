using System;
using FramePFX.Core;

namespace FramePFX.Timeline.Layer.Clips {
    public class ClipViewModel : BaseViewModel {
        public LayerViewModel Layer { get; }

        private long frameBegin;
        public long FrameBegin {
            get => this.frameBegin;
            set => this.RaisePropertyChanged(ref this.frameBegin, value);
        }

        private long frameDuration;
        public long FrameDuration {
            get => this.frameDuration;
            set => this.RaisePropertyChanged(ref this.frameDuration, value);
        }

        public long FrameEndIndex {
            get => this.FrameBegin + this.FrameDuration;
            set {
                long duration = value - this.FrameBegin;
                if (duration < 0) {
                    throw new ArgumentException($"FrameEndIndex cannot be below FrameBegin ({value} < {this.FrameBegin})");
                }

                this.FrameDuration = duration;
            }
        }

        public TimelineClipControl Control { get; set; }

        public ClipViewModel(LayerViewModel layer) {
            this.Layer = layer;
        }
    }
}
