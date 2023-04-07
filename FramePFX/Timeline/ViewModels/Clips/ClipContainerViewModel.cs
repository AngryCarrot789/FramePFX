using System;
using System.Collections.Generic;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.AdvancedContextService;
using FramePFX.Core.AdvancedContextService.Base;

namespace FramePFX.Timeline.Layer.Clips {
    /// <summary>
    /// A container for a clip. This is used only to contain a "clip". Checking whether
    /// this is a video or an audio clip can be done by accessing <see cref="Content"/>
    /// </summary>
    public sealed class ClipContainerViewModel : BaseViewModel, IContextProvider {
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
                this.RaisePropertyChanged(nameof(this.FrameBegin));
                this.RaisePropertyChanged(nameof(this.FrameDuration));
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
        public LayerViewModel Layer { get; set; }

        /// <summary>
        /// The content of this clip
        /// </summary>
        public ClipViewModel Content { get; set; }

        public ICommand RenameCommand { get; }
        public ICommand DeleteCommand { get; }

        public ClipContainerViewModel() {
            this.RenameCommand = new RelayCommand(this.RenameAction);
            this.DeleteCommand = new RelayCommand(() => {
                this.Layer.DeleteClip(this);
            });
        }

        private void RenameAction() {
            string newName = CoreIoC.UserInput.ShowSingleInputDialog("Rename clip", "Input a new clip name:", this.Name ?? "");
            if (newName != null) {
                this.Name = newName;
            }
        }

        public static void SetClipContent(ClipContainerViewModel container, ClipViewModel clip) {
            ClipViewModel oldClip = container.Content;
            if (oldClip != null) {
                container.Content = null;
                // container.OnClipRemoved(oldClip, clip != null);
            }

            if (clip != null) {
                container.Content = clip;
                // container.OnClipAdded(clip);
            }
        }

        public bool IntersectsFrameAt(long frame) {
            long begin = this.FrameBegin;
            long duration = this.FrameDuration;
            return frame >= begin && frame < (begin + duration);
        }

        public void MarkForRender() {
            if (IoC.VideoEditor.PlaybackView.IsPlaying) {
                return;
            }

            if (this.Layer != null && IoC.VideoEditor.IsReadyForRender()) {
                this.Layer.Timeline.ScheduleRender(false);
            }
        }

        public List<IContextEntry> GetContext(List<IContextEntry> list) {
            list.Add(new CommandContextEntry("Rename", this.RenameCommand));
            list.Add(new CommandContextEntry("Delete", this.DeleteCommand));
            if (this.Content is IContextProvider provider) {
                List<IContextEntry> contentList = provider.GetContext(new List<IContextEntry>());
                if (contentList.Count > 0) {
                    list.Add(ContextEntrySeparator.Instance);
                    list.AddRange(contentList);
                }
            }

            return list;
        }
    }
}
