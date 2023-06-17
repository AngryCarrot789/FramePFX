using System;
using FramePFX.Core.Editor.ViewModels;

namespace FramePFX.Core.Automation.ViewModels {
    public class AutomationEngineViewModel {
        public AutomationEngine Model { get; }

        public ProjectViewModel Project { get; }

        public AutomationEngineViewModel(ProjectViewModel project, AutomationEngine model) {
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}