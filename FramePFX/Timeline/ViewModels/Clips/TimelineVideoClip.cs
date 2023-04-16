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
        protected long frameBegin;
        protected long frameDuration;
        protected long frameMediaOffset;

        /// <summary>
        /// The frame where this clip physically begins on the timeline layer
        /// </summary>
        public long FrameBegin {
            get => this.frameBegin;
            set => this.RaisePropertyChanged(ref this.frameBegin, value, this.InvalidateRender);
        }

        /// <summary>
        /// The duration of this clip, relative to <see cref="FrameBegin"/>
        /// </summary>
        public long FrameDuration {
            get => this.frameDuration;
            set => this.RaisePropertyChanged(ref this.frameDuration, value, this.InvalidateRender);
        }

        /// <summary>
        /// Sets or sets this clip's end index, which is equal to <see cref="FrameBegin"/> + <see cref="FrameDuration"/>
        /// </summary>
        /// <exception cref="ArgumentException">New frame end index is less than <see cref="FrameBegin"/></exception>
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

        /// <summary>
        /// The offset of this clip's media. This is modified when the left thumb is dragged or a clip is cut
        /// <para>
        /// Adding this to <see cref="FrameBegin"/> results in the "media" frame begin, relative to the timeline.
        /// </para>
        /// <para>
        /// When the left thumb is dragged towards the left, this is incremented (may become positive).
        /// When the clip is dragged towards the right, this is decremented (may become negative).
        /// When the clip is split in half, the left clip's frame media offset is untouched, but
        /// the right side is decremented by the duration of the left clip
        /// </para>
        /// </summary>
        public long FrameMediaOffset {
            get => this.frameMediaOffset;
            set => this.RaisePropertyChanged(ref this.frameMediaOffset, value);
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

        public VideoTimelineLayer VideoLayer => (VideoTimelineLayer) base.Layer;

        protected TimelineVideoClip() {

        }

        public bool IntersectsFrameAt(long frame) {
            long begin = this.FrameBegin;
            long duration = this.FrameDuration;
            return frame >= begin && frame < (begin + duration);
        }

        /// <summary>
        /// The main render function for a timeline clip
        /// </summary>
        /// <param name="vp">The viewport that's being rendered into</param>
        /// <param name="frame">The current frame that needs to be rendered</param>
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

        public override void LoadDataIntoClone(BaseTimelineClip clone) {
            base.LoadDataIntoClone(clone);
            if (clone is TimelineVideoClip clip) {
                clip.frameBegin = this.frameBegin;
                clip.frameDuration = this.frameDuration;
                clip.frameMediaOffset = this.frameMediaOffset;
            }
        }
    }
}
