using System.Collections.Generic;
using System.Linq;

namespace FramePFX.Editor.Timeline.New {
    public class PFXTimelineLayer {
        public List<PFXClip> Clips { get; }

        public PFXTimeline Timeline { get; }

        public PFXTimelineLayer(PFXTimeline timeline) {
            this.Timeline = timeline;
            this.Clips = new List<PFXClip>();
        }

        public IEnumerable<PFXClip> GetClipsAtFrame(long frame) {
            return this.Clips.Where(clip => clip.IntersectsFrameAt(frame));
        }
    }
}