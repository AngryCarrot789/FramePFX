using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;
using FramePFX.PropertyEditing;
using FramePFX.PropertyEditing.Editors;

namespace FramePFX.Editor.PropertyEditors.Clips {
    public class TimerClipDataEditorViewModel : HistoryAwarePropertyEditorViewModel {
        public TimerClipViewModel SingleSelection => (TimerClipViewModel) this.Handlers[0];
        public IEnumerable<TimerClipViewModel> Clips => this.Handlers.Cast<TimerClipViewModel>();

        private bool? useClipStartTime;
        public bool? UseClipStartTime {
            get => this.useClipStartTime;
            set {
                this.useClipStartTime = value;
                this.RaisePropertyChanged();
                if (value is bool bb) {
                    foreach (TimerClipViewModel clip in this.Clips) {
                        clip.UseClipStartTime = bb;
                    }
                }
            }
        }

        private bool? useClipEndTime;
        public bool? UseClipEndTime {
            get => this.useClipEndTime;
            set {
                this.useClipEndTime = value;
                this.RaisePropertyChanged();
                if (value is bool bb) {
                    foreach (TimerClipViewModel clip in this.Clips) {
                        clip.UseClipEndTime = bb;
                    }
                }
            }
        }

        private TimeSpan startTime;
        public TimeSpan StartTime {
            get => this.startTime;
            set {
                this.startTime = value;
                this.RaisePropertyChanged();
                foreach (TimerClipViewModel clip in this.Clips) {
                    clip.StartTime = value;
                }
            }
        }

        private TimeSpan endTime;
        public TimeSpan EndTime {
            get => this.endTime;
            set {
                this.endTime = value;
                this.RaisePropertyChanged();
                foreach (TimerClipViewModel clip in this.Clips) {
                    clip.EndTime = value;
                }
            }
        }


        private string fontFamily;
        public string FontFamily {
            get => this.fontFamily;
            set {
                this.fontFamily = value;
                this.RaisePropertyChanged();
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
            this.useClipStartTime = GetEqualValue(this.Handlers, x => ((TimerClipViewModel) x).UseClipStartTime, out bool d) ? d : (bool?) null;
            this.RaisePropertyChanged(nameof(this.UseClipStartTime));
        }

        public void RequeryUseClipEndTime() {
            this.useClipEndTime = GetEqualValue(this.Handlers, x => ((TimerClipViewModel) x).UseClipEndTime, out bool d) ? d : (bool?) null;
            this.RaisePropertyChanged(nameof(this.UseClipEndTime));
        }

        public void RequeryStartTime() {
            this.startTime = GetEqualValue(this.Handlers, x => ((TimerClipViewModel) x).StartTime, out TimeSpan d) ? d : default;
            this.RaisePropertyChanged(nameof(this.StartTime));
        }

        public void RequeryEndTime() {
            this.endTime = GetEqualValue(this.Handlers, x => ((TimerClipViewModel) x).EndTime, out TimeSpan d) ? d : default;
            this.RaisePropertyChanged(nameof(this.EndTime));
        }

        public void RequeryFontFamiltyFromHandlers() {
            this.fontFamily = GetEqualValue(this.Handlers, x => ((TimerClipViewModel) x).FontFamily, out string d) ? d : DifferentValueText;
            this.RaisePropertyChanged(nameof(this.FontFamily));
        }
    }

    // Use different types because it's more convenient to create DataTemplates;
    // no need for a template selector to check the mode

    public class TimerClipDataSingleEditorViewModel : TimerClipDataEditorViewModel {
        public sealed override HandlerCountMode HandlerCountMode => HandlerCountMode.Single;
    }

    public class TimerClipDataMultiEditorViewModel : TimerClipDataEditorViewModel {
        public sealed override HandlerCountMode HandlerCountMode => HandlerCountMode.Multi;
    }
}