
using FramePFX.Editors.Timelines.Tracks;

namespace FramePFX.Editors.Timelines {
    public static class TimelineUtils {
        public static int GetIndexInTimeline(this Track track) {
            return track.IndexInTimeline;
        }
    }
}