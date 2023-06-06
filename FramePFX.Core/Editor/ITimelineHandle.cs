using System.Collections.Generic;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;

namespace FramePFX.Core.Editor {
    public interface ITimelineHandle : IHasZoom {
        long MaxDuration { get; set; }

        IEnumerable<VideoClipViewModel> GetSelectedClips();
    }
}