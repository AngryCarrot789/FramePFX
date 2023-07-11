using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timelines {
    public readonly struct SelectionRect {
        public static readonly SelectionRect Empty = default;

        public readonly FrameSpan Span;
        public readonly int TrackIndex;
        public readonly int TrackCount;

        public bool IsEmpty => this.Span.Duration > 0 && this.TrackCount > 0;

        public SelectionRect(FrameSpan span, int trackIndex, int trackCount) {
            this.Span = span;
            this.TrackIndex = trackIndex;
            this.TrackCount = trackCount;
        }
    }

    public readonly struct TrackInfo {
        public readonly int Index;
        public readonly double Height;
    }
}