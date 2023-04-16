using FramePFX.Timeline.ViewModels.Clips;

namespace FramePFX.Timeline.ViewModels.Layer.Removals {
    public readonly struct VideoClipModification {
        /// <summary>
        /// The clip's old span
        /// </summary>
        public FrameSpan OldSpan { get; }
        /// <summary>
        /// The left-part of the clip that may have been split. Duration may be 0 if there is no left-split clip
        /// </summary>
        public FrameSpan NewSpan1 { get; }

        /// <summary>
        /// The right-part of the clip that may have been split. Duration may be 0 if there is no right-split clip
        /// </summary>
        public FrameSpan NewSpan2 { get; }

        public TimelineVideoClip Clip { get; }

        public bool IsRemoved => this.NewSpan1.Duration == 0 && this.NewSpan2.Duration == 0;

        public VideoClipModification(FrameSpan oldSpan, FrameSpan newSpan1, FrameSpan newSpan2, TimelineVideoClip clip) {
            this.OldSpan = oldSpan;
            this.NewSpan1 = newSpan1;
            this.NewSpan2 = newSpan2;
            this.Clip = clip;
        }

        public static VideoClipModification Create(TimelineVideoClip clip, long cutBegin, long cutDuration) {
            long cutEndIndex = cutBegin + cutDuration;
        }
    }
}