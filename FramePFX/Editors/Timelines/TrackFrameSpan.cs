using FramePFX.Editors.Timelines.Clips;

namespace FramePFX.Editors.Timelines {
    /// <summary>
    /// A struct that stores a frame span and track index, which is basically how a clip can be represented
    /// </summary>
    public readonly struct TrackFrameSpan {
        /// <summary>
        /// Returns an invalid track frame span with a track index of -1
        /// </summary>
        public static TrackFrameSpan Invalid => new TrackFrameSpan(default, -1);

        public readonly FrameSpan Span;
        public readonly int TrackIndex;

        public TrackFrameSpan(FrameSpan span, int trackIndex) {
            this.Span = span;
            this.TrackIndex = trackIndex;
        }

        public TrackFrameSpan(Clip clip) {
            this.Span = clip.FrameSpan;
            this.TrackIndex = clip.Track?.IndexInTimeline ?? -1;
        }
    }
}