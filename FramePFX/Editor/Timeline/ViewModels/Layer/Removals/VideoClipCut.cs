using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Utils;
using FramePFX.Editor.Timeline.ViewModels.Clips;

namespace FramePFX.Editor.Timeline.ViewModels.Layer.Removals {
    public readonly struct VideoClipCut {
        /// <summary>
        /// The clip's old span
        /// </summary>
        public ClipSpan OldSpan { get; }

        // The left and right spans represent the space that a clip can take up

        /// <summary>
        /// The left-part of the cut
        /// </summary>
        public ClipSpan? CutLeft { get; }

        /// <summary>
        /// The right-part of the cut
        /// </summary>
        public ClipSpan? CutRight { get; }

        public PFXVideoClipViewModel Clip { get; }

        public bool IsClipRemoved {
            get {
                if (this.CutLeft.HasValue && this.CutRight.HasValue) {
                    return this.CutLeft.Value.Duration == 0 && this.CutRight.Value.Duration == 0;
                }

                return false;
            }
        }

        /// <summary>
        /// In the event that this is a double-split, where span left and right have non-empty values, this returns the original width of
        /// the split. Will be 0 for a clip slice, non-negative for a duration cut, and will be -1 when span left or right are empty
        /// </summary>
        public long SplitWidth {
            get {
                if (this.CutLeft.HasValue && this.CutRight.HasValue) {
                    return this.CutRight.Value.Begin - this.CutLeft.Value.Begin;
                }

                return -1;
            }
        }

        public VideoClipCut(ClipSpan oldSpan, ClipSpan? cutLeft, ClipSpan? cutRight, PFXVideoClipViewModel clip) {
            this.OldSpan = oldSpan;
            this.CutLeft = cutLeft;
            this.CutRight = cutRight;
            this.Clip = clip;
        }
    }
}