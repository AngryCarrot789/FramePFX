using System;
using FramePFX.Core;

namespace FramePFX.Timeline.Layer.Clips {
    /// <summary>
    /// A container for a clip
    /// </summary>
    public abstract class ClipContainerViewModel : BaseViewModel {
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

        public IClipContainerHandle ContainerHandle { get; set; }

        /// <summary>
        /// The layer that this clip container is currently in. Should be null if the clip is not yet in a layer
        /// </summary>
        public LayerViewModel Layer { get; set; }

        /// <summary>
        /// The content of this clip
        /// </summary>
        public ClipViewModel ClipContent { get; set; }

        protected ClipContainerViewModel() {

        }

        public static void SetClipContent(ClipContainerViewModel container, ClipViewModel clip) {
            ClipViewModel oldClip = container.ClipContent;
            if (oldClip != null) {
                container.ClipContent = null;
                container.OnRemovedFromContainer(oldClip, clip != null);
            }

            if (clip != null) {
                container.ClipContent = clip;
                container.OnAddedToContainer(clip);
            }
        }

        public virtual void OnRemovedFromContainer(ClipViewModel clip, bool isReplace) {
            if (clip.Container != null) {
                ClipViewModel.SetContainer(clip, null);
            }
        }

        public virtual void OnAddedToContainer(ClipViewModel clip) {
            if (clip.Container != this) {
                ClipViewModel.SetContainer(clip, this);
            }
        }

        public bool IntersectsFrameAt(long frame) {
            long begin = this.FrameBegin;
            long duration = this.FrameDuration;
            return frame >= begin && frame < (begin + duration);
        }

        public void MarkForRender() {
            if (this.Layer != null && (IoC.VideoEditor?.IsReadyForRender() ?? false)) {
                this.Layer.Timeline.ScheduleRender(true);
            }
        }
    }
}
