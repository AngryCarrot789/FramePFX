using System;
using System.Collections.Generic;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.AdvancedContextService;
using FramePFX.Core.AdvancedContextService.Base;
using FramePFX.Core.AdvancedContextService.Commands;
using FramePFX.Render;
using FramePFX.Timeline.Layer.Clips;
using FramePFX.Timeline.ViewModels.Layer;

namespace FramePFX.Timeline.ViewModels.Clips {
    /// <summary>
    /// A container for a clip. This is used only to contain a "clip". Checking whether
    /// this is a video or an audio clip can be done by accessing <see cref="Content"/>
    /// </summary>
    public abstract class TimelineVideoClip : BaseTimelineClip, IContextProvider, IVideoClip {
        protected bool ignoreMarkRender;

        private long frameBegin;
        public long FrameBegin {
            get => this.frameBegin;
            set => this.RaisePropertyChanged(ref this.frameBegin, value, this.InvalidateRender);
        }

        private long frameDuration;
        public long FrameDuration {
            get => this.frameDuration;
            set => this.RaisePropertyChanged(ref this.frameDuration, value, this.InvalidateRender);
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

                this.InvalidateRender();
            }
        }

        protected TimelineVideoClip() {

        }

        public bool IntersectsFrameAt(long frame) {
            long begin = this.FrameBegin;
            long duration = this.FrameDuration;
            return frame >= begin && frame < (begin + duration);
        }

        public abstract void Render(IViewPort vp, long frame);

        public void InvalidateRender() {
            this.InvalidateRender(false);
        }

        public void InvalidateRender(bool useCurrentThread) {
            if (this.ignoreMarkRender || this.Layer == null) {
                return;
            }

            ViewportPlayback editor = this.Layer.Timeline.PlaybackViewport;
            if (!editor.IsPlaying && editor.IsReadyForRender()) {
                this.Layer.Timeline.ScheduleRender(useCurrentThread);
            }
        }

        public List<IContextEntry> GetContext(List<IContextEntry> list) {
            list.Add(new CommandContextEntry("Rename", this.RenameCommand));
            list.Add(new CommandContextEntry("Delete", this.DeleteCommand));
            return list;
        }
    }
}
