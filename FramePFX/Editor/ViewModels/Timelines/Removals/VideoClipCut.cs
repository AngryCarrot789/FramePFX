using FramePFX.Editor.ViewModels.Timelines.VideoClips;
using FramePFX.Utils;

namespace FramePFX.Editor.ViewModels.Timelines.Removals
{
    public readonly struct VideoClipCut
    {
        /// <summary>
        /// The clip's old span
        /// </summary>
        public FrameSpan OldSpan { get; }

        // The left and right spans represent the space that a clip can take up

        /// <summary>
        /// The left-part of the cut
        /// </summary>
        public FrameSpan? CutLeft { get; }

        /// <summary>
        /// The right-part of the cut
        /// </summary>
        public FrameSpan? CutRight { get; }

        public VideoClipViewModel Clip { get; }

        public bool IsClipRemoved
        {
            get
            {
                if (this.CutLeft.HasValue && this.CutRight.HasValue)
                {
                    return this.CutLeft.Value.Duration == 0 && this.CutRight.Value.Duration == 0;
                }

                return false;
            }
        }

        /// <summary>
        /// In the event that this is a double-split, where span left and right have non-empty values, this returns the original width of
        /// the split. Will be 0 for a clip slice, non-negative for a duration cut, and will be -1 when span left or right are empty
        /// </summary>
        public long SplitWidth
        {
            get
            {
                if (this.CutLeft.HasValue && this.CutRight.HasValue)
                {
                    return this.CutRight.Value.Begin - this.CutLeft.Value.Begin;
                }

                return -1;
            }
        }

        public VideoClipCut(FrameSpan oldSpan, FrameSpan? cutLeft, FrameSpan? cutRight, VideoClipViewModel clip)
        {
            this.OldSpan = oldSpan;
            this.CutLeft = cutLeft;
            this.CutRight = cutRight;
            this.Clip = clip;
        }
    }
}