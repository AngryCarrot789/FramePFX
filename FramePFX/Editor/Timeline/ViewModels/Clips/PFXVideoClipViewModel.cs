using System;
using System.Collections.Generic;
using FramePFX.Core.AdvancedContextService;
using FramePFX.Editor.Timeline.Layer.Clips;
using FramePFX.Editor.Timeline.Utils;
using FramePFX.Editor.Timeline.ViewModels.Layer;
using FramePFX.Editor.ViewModels;
using FramePFX.Render;

namespace FramePFX.Editor.Timeline.ViewModels.Clips {
    /// <summary>
    /// The base view model class for video-based clips, such as images, videos, gifs, etc
    /// </summary>
    public abstract class VideoClipViewModel : BaseClipViewModel, IContextProvider, IVideoClip {
        protected bool ignoreMarkRender;
        protected long frameBegin;
        protected long frameDuration;
        protected long frameMediaOffset;

        /// <summary>
        /// The frame where this clip physically begins on the timeline layer
        /// </summary>
        public long FrameBegin {
            get => this.frameBegin;
            set {
                this.RaisePropertyChanged(ref this.frameBegin, value);
                this.InvalidateRenderForPropertyChanged();
            }
        }

        /// <summary>
        /// The duration of this clip, relative to <see cref="FrameBegin"/>
        /// </summary>
        public long FrameDuration {
            get => this.frameDuration;
            set {
                this.RaisePropertyChanged(ref this.frameDuration, value);
                this.InvalidateRenderForPropertyChanged();
            }
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
        // TODO: Implement this with the left thumb + media clips. Should it be negative or positive when dragged right?
        public long FrameMediaOffset {
            get => this.frameMediaOffset;
            set => this.RaisePropertyChanged(ref this.frameMediaOffset, value);
        }

        public FrameSpan Span {
            get => new FrameSpan(this.frameBegin, this.frameDuration);
            set {
                this.frameBegin = value.Begin;
                this.frameDuration = value.Duration;
                this.RaisePropertyChanged(nameof(this.FrameBegin));
                this.RaisePropertyChanged(nameof(this.FrameDuration));
                this.InvalidateRenderForPropertyChanged();
            }
        }

        public PFXVideoLayer VideoLayer => (PFXVideoLayer) base.Layer;

        protected VideoClipViewModel() {

        }

        public override bool IntersectsFrameAt(long frame) {
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

        public void InvalidateRenderForPropertyChanged() {
            if (!this.ignoreMarkRender) {
                this.InvalidateRender(false);
            }
        }

        public void InvalidateRender(bool useCurrentThread = false) {
            if (this.ignoreMarkRender || this.Layer == null) {
                return;
            }

            PFXViewportPlayback editor = this.Layer.Timeline.PlaybackViewport;
            if (!editor.IsPlaying && editor.IsReadyForRender()) {
                this.Layer.Timeline.ScheduleRender(useCurrentThread);
            }
        }

        public void GetContext(List<IContextEntry> list) {
            list.Add(new CommandContextEntry("Rename", this.RenameCommand));
            list.Add(new CommandContextEntry("Delete", this.DeleteCommand));
        }

        public override void LoadDataIntoClone(BaseClipViewModel clone) {
            base.LoadDataIntoClone(clone);
            if (clone is VideoClipViewModel clip) {
                clip.frameBegin = this.frameBegin;
                clip.frameDuration = this.frameDuration;
                clip.frameMediaOffset = this.frameMediaOffset;
            }
        }

        public override void OnRemoving(PFXTimelineLayer layer) {
            this.ignoreMarkRender = true;
            try {
                base.OnRemoving(layer);
            }
            finally {
                this.ignoreMarkRender = false;
            }
        }
    }
}
