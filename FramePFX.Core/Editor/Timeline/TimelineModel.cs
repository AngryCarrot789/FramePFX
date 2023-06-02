using System.Collections.Generic;

namespace FramePFX.Core.Editor.Timeline {
    public class TimelineModel {
        public ProjectModel Project { get; }

        public List<TimelineLayerModel> Layers { get; }

        public TimelineModel(ProjectModel project) {
            this.Project = project;
            this.Layers = new List<TimelineLayerModel>();
        }
    }
}