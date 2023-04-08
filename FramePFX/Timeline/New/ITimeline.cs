using System.Collections.Generic;

namespace FramePFX.Timeline.New {
    public interface ITimeline {
        double UnitZoom { get; set; }

        long MaxDuration { get; set; }

        IEnumerable<IClipContainer> Clips { get; }

        IEnumerable<IClipContainer> SelectedClips { get; }
    }
}