using System.Collections.Generic;
using FramePFX.Timeline.ViewModels.Clips;

namespace FramePFX.Timeline {
    public interface ITimelineHandle : IHasZoom {
        long MaxDuration { get; set; }

        IEnumerable<ClipContainer> GetSelectedClips();
    }
}