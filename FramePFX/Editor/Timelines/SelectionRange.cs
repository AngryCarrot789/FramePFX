using FramePFX.Utils;

namespace FramePFX.Editor.Timelines {
    public readonly struct SelectionRange {
        public readonly FrameSpan Span;

        public bool IsEmpty => this.Span.IsEmpty;

        public SelectionRange(FrameSpan span) {
            this.Span = span;
        }
    }
}