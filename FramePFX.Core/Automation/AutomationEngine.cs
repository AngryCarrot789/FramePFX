using System.Collections.Generic;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.Timeline;

namespace FramePFX.Core.Automation {
    public class AutomationEngine {
        public ProjectModel Project { get; }

        public AutomationEngine(ProjectModel project) {
            this.Project = project;
        }

        public void UpdateFrame(IEnumerable<LayerModel> layers, long frame) {
            
        }
    }
}