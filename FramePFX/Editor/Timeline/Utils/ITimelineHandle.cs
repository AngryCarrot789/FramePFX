using System.Collections.Generic;
using FramePFX.Editor.Timeline.ViewModels.Clips;

namespace FramePFX.Editor.Timeline.Utils {
    public interface ITimelineHandle : IHasZoom {
        long MaxDuration { get; set; }

        IEnumerable<PFXVideoClipViewModel> GetSelectedClips();
    }
}