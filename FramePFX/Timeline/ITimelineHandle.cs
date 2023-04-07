using System.Collections.Generic;
using FramePFX.Timeline.ViewModels.Clips;

namespace FramePFX.Timeline {
    public interface ITimelineHandle {
        IEnumerable<ClipContainerViewModel> GetSelectedClips();
    }
}