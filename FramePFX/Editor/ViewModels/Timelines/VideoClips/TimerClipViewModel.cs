using System;
using FramePFX.Editor.Timelines.VideoClips;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class TimerClipViewModel : VideoClipViewModel {
        public new TimerClip Model => (TimerClip) ((ClipViewModel) this).Model;

        public bool UseClipStartTime {
            get => this.Model.UseClipStartTime;
            set {
                this.RaisePropertyChanged(ref this.Model.UseClipStartTime, value);
                this.Model.InvalidateRender();
            }
        }

        public bool UseClipEndTime {
            get => this.Model.UseClipEndTime;
            set {
                this.RaisePropertyChanged(ref this.Model.UseClipEndTime, value);
                this.Model.InvalidateRender();
            }
        }

        public TimeSpan StartTime {
            get => this.Model.StartTime;
            set {
                this.RaisePropertyChanged(ref this.Model.StartTime, value);
                this.Model.InvalidateRender();
            }
        }

        public TimeSpan EndTime {
            get => this.Model.EndTime;
            set {
                this.RaisePropertyChanged(ref this.Model.EndTime, value);
                this.Model.InvalidateRender();
            }
        }

        public string FontFamily {
            get => this.Model.FontFamily;
            set {
                this.Model.FontFamily = value;
                this.RaisePropertyChanged();
                this.Model.InvalidateRender();
            }
        }

        public TimerClipViewModel(TimerClip model) : base(model) {

        }
    }
}