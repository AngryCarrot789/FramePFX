using System.Collections.Generic;
using FramePFX.Core.Editor.Timeline.Clip;

namespace FramePFX.Core.Editor.Timeline {
    public class TimelineLayerModel {
        public TimelineModel Timeline { get; }

        public List<ClipModel> Clips { get; }

        public TimelineLayerModel(TimelineModel timeline) {
            this.Timeline = timeline;
            this.Clips = new List<ClipModel>();
        }
    }
}