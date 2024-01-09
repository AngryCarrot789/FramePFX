using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;
using FramePFX.PropertyEditing.Editors;

namespace FramePFX.Editor.PropertyEditors.Clips {
    public class TimerClipDataEditorViewModel : HistoryAwarePropertyEditorViewModel {
        public TimerClipViewModel SingleSelection => (TimerClipViewModel) this.Handlers[0];
        public IEnumerable<TimerClipViewModel> Clips => this.Handlers.Cast<TimerClipViewModel>();

        private bool? useClipStartTime;
        public bool? UseClipStartTime {
            get => this.useClipStartTime;
            set {
                this.RaisePropertyChanged(ref this.useClipStartTime, value);
                if (value is bool val) {
                    foreach (TimerClipViewModel clip in this.Clips) {
                        clip.UseClipStartTime = val;
                    }
                }
            }
        }

        private bool? useClipEndTime;
        public bool? UseClipEndTime {
            get => this.useClipEndTime;
            set {
                this.RaisePropertyChanged(ref this.useClipEndTime, value);
                if (value is bool val) {
                    foreach (TimerClipViewModel clip in this.Clips) {
                        clip.UseClipEndTime = val;
                    }
                }
            }
        }

        private TimeSpan startTime;
        public TimeSpan StartTime {
            get => this.startTime;
            set {
                this.RaisePropertyChanged(ref this.startTime, value);
                foreach (TimerClipViewModel clip in this.Clips) {
                    clip.StartTime = value;
                }
            }
        }

        private TimeSpan endTime;
        public TimeSpan EndTime {
            get => this.endTime;
            set {
                this.RaisePropertyChanged(ref this.endTime, value);
                foreach (TimerClipViewModel clip in this.Clips) {
                    clip.EndTime = value;
                }
            }
        }

        private string fontFamily = "Consolas";
        public string FontFamily {
            get => this.fontFamily;
            set {
                this.RaisePropertyChanged(ref this.fontFamily, value);
                foreach (TimerClipViewModel clip in this.Clips) {
                    clip.FontFamily = value;
                }
            }
        }

        public static string DifferentValueText => IoC.Translator.GetString("S.PropertyEditor.NamedObject.DifferingDisplayNames");

        public TimerClipDataEditorViewModel() : base(typeof(TimerClipViewModel)) {
        }

        static TimerClipDataEditorViewModel() {
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            this.RequeryUseClipStartTime();
            this.RequeryUseClipEndTime();
            this.RequeryStartTime();
            this.RequeryEndTime();
            this.RequeryFontFamiltyFromHandlers();
        }

        public void RequeryUseClipStartTime() {
            this.RaisePropertyChanged(ref this.useClipStartTime, GetEqualValue(this.Handlers, x => ((TimerClipViewModel) x).UseClipStartTime, out bool d) ? d : (bool?) null, nameof(this.UseClipStartTime));
        }

        public void RequeryUseClipEndTime() {
            this.RaisePropertyChanged(ref this.useClipEndTime, GetEqualValue(this.Handlers, x => ((TimerClipViewModel) x).UseClipEndTime, out bool d) ? d : (bool?) null, nameof(this.UseClipEndTime));
        }

        public void RequeryStartTime() {
            this.RaisePropertyChanged(ref this.startTime, GetEqualValue(this.Handlers, x => ((TimerClipViewModel) x).StartTime, out TimeSpan d) ? d : default, nameof(this.StartTime));
        }

        public void RequeryEndTime() {
            this.RaisePropertyChanged(ref this.endTime, GetEqualValue(this.Handlers, x => ((TimerClipViewModel) x).EndTime, out TimeSpan d) ? d : default, nameof(this.EndTime));
        }

        public void RequeryFontFamiltyFromHandlers() {
            this.RaisePropertyChanged(ref this.fontFamily, GetEqualValue(this.Handlers, x => ((TimerClipViewModel) x).FontFamily, out string d) ? d : DifferentValueText, nameof(this.FontFamily));
        }
    }
}