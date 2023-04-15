using System;
using System.Collections.Generic;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.AdvancedContextService;
using FramePFX.Core.AdvancedContextService.Base;
using FramePFX.Core.AdvancedContextService.Commands;
using FramePFX.Timeline.Layer.Clips;
using FramePFX.Timeline.ViewModels.Layer;

namespace FramePFX.Timeline.ViewModels.Clips {
    /// <summary>
    /// A container for a clip. This is used only to contain a "clip". Checking whether
    /// this is a video or an audio clip can be done by accessing <see cref="Content"/>
    /// </summary>
    public class VideoClip : BaseTimelineClip, IContextProvider {
        protected bool ignoreMarkRender;

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
                this.frameBegin = value.Begin;
                this.frameDuration = value.Duration;
                try {
                    this.ignoreMarkRender = true;
                    this.RaisePropertyChanged(nameof(this.FrameBegin));
                    this.RaisePropertyChanged(nameof(this.FrameDuration));
                }
                finally {
                    this.ignoreMarkRender = false;
                }

                this.MarkForRender();
            }
        }

        private string name;
        public string Name {
            get => this.name;
            set => this.RaisePropertyChanged(ref this.name, value);
        }

        /// <summary>
        /// A reference to the actual UI element clip container
        /// </summary>
        public IClipContainerHandle Handle { get; set; }

        /// <summary>
        /// The layer that this clip container is currently in. Should be null if the clip is not yet in a layer
        /// </summary>
        public TimelineLayer TimelineLayer { get; set; }

        public ICommand RenameCommand { get; }
        public ICommand DeleteCommand { get; }

        public VideoClip() {
            this.RenameCommand = new RelayCommand(this.RenameAction);
            this.DeleteCommand = new RelayCommand(() => {
                this.TimelineLayer.DeleteClip(this);
            });
        }

        private void RenameAction() {
            string newName = CoreIoC.UserInput.ShowSingleInputDialog("Rename clip", "Input a new clip name:", this.Name ?? "");
            if (newName != null) {
                this.Name = newName;
            }
        }

        public bool IntersectsFrameAt(long frame) {
            long begin = this.FrameBegin;
            long duration = this.FrameDuration;
            return frame >= begin && frame < (begin + duration);
        }

        public void MarkForRender() {
            if (this.ignoreMarkRender || IoC.VideoEditor.PlaybackView.IsPlaying) {
                return;
            }

            if (this.TimelineLayer != null && IoC.VideoEditor.IsReadyForRender()) {
                this.TimelineLayer.Timeline.ScheduleRender(false);
            }
        }

        public List<IContextEntry> GetContext(List<IContextEntry> list) {
            list.Add(new CommandContextEntry("Rename", this.RenameCommand));
            list.Add(new CommandContextEntry("Delete", this.DeleteCommand));
            return list;
        }
    }
}
