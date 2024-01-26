using FramePFX.Editors.Timelines.Clips;

namespace FramePFX.Editors.Timelines {
    /// <summary>
    /// Stores a track index and frame position
    /// </summary>
    public readonly struct TrackPoint {
        public static TrackPoint Invalid => new TrackPoint(0, -1);

        public readonly long Frame;
        public readonly int TrackIndex;

        public TrackPoint(long frame, int trackIndex) {
            this.Frame = frame;
            this.TrackIndex = trackIndex;
        }

        public TrackPoint(Clip clip) : this(clip, clip.FrameSpan.Begin) {
        }

        public TrackPoint(Clip clip, long frame) {
            this.Frame = frame;
            this.TrackIndex = clip.Track?.IndexInTimeline ?? -1;
        }
    }
}