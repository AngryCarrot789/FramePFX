using FramePFX.Editor.Timeline.New.Layers;
using FramePFX.Editor.Timeline.Utils;
using FramePFX.Render;

namespace FramePFX.Editor.Timeline.New.Clips {
    // heh XVideoClip...
    public abstract class PFXVideoClip : PFXClip {
        /// <summary>
        /// This clip's start location (in frames)
        /// </summary>
        public long FrameBegin { get; set; }

        /// <summary>
        /// This clip's duration (in frames)
        /// </summary>
        public long FrameDuration { get; set; }

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
        public long FrameMediaOffset { get; set; }

        public FrameSpan Span {
            get => new FrameSpan(this.FrameBegin, this.FrameDuration);
            set {
                this.FrameBegin = value.Begin;
                this.FrameDuration = value.Duration;
            }
        }

        public PFXVideoLayer VideoLayer => (PFXVideoLayer) base.Layer;

        protected PFXVideoClip() {

        }

        public override bool IntersectsFrameAt(long frame) {
            long begin = this.FrameBegin;
            return frame >= begin && frame < (begin + this.FrameDuration);
        }

        /// <summary>
        /// The main render function for a timeline clip
        /// </summary>
        /// <param name="vp">The viewport that's being rendered into</param>
        /// <param name="frame">The current frame that needs to be rendered</param>
        public abstract void Render(IViewPort vp, long frame);

        public override void LoadDataIntoClone(PFXClip clone) {
            base.LoadDataIntoClone(clone);
            if (clone is PFXVideoClip clip) {
                clip.FrameBegin = this.FrameBegin;
                clip.FrameDuration = this.FrameDuration;
                clip.FrameMediaOffset = this.FrameMediaOffset;
            }
        }
    }
}