using System;

namespace FramePFX.Core.Timeline {
    public class ClipViewModel : BaseViewModel {
        private long frameBegin;
        public long FrameBegin {
            get => this.frameBegin;
            set => this.RaisePropertyChanged(ref this.frameBegin, value, this.MarkForRender);
        }

        private long frameDuration;
        public long FrameDuration {
            get => this.frameDuration;
            set => this.RaisePropertyChanged(ref this.frameDuration, value, this.MarkForRender);
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

        public FrameSpan Span {
            get => new FrameSpan(this.FrameBegin, this.FrameDuration);
            set {
                this.FrameBegin = value.Begin;
                this.FrameDuration = value.Duration;
            }
        }

        private string name;
        public string Name {
            get => this.name;
            set => this.RaisePropertyChanged(ref this.name, value);
        }

        public IClipHandle Control { get; set; }

        public LayerViewModel Layer { get; set; }

        public ClipViewModel() {

        }

        public bool IntersectsFrameAt(long frame) {
            long begin = this.FrameBegin;
            long duration = this.FrameDuration;
            return frame >= begin && frame < (begin + duration);
        }

        private void MarkForRender() {
            if (this.Layer != null && (IoC.Editor.MainViewPort?.IsReadyForRender ?? false)) {
                this.Layer.Timeline.ScheduleRender(true);
            }
        }
    }
}
