using System;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline;
using NAudio.Wave;

namespace FramePFX.Core.Automation.ViewModels {
    public class AutomationEngineViewModel {
        public AutomationEngine Model { get; }

        public ProjectViewModel Project { get; }

        public AutomationEngineViewModel(ProjectViewModel project, AutomationEngine model) {
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public void UpdateOnPlayHeadMoved(bool isRendering) {
            this.Project.Timeline.UpdateAutomationValues(isRendering, this.Project.Timeline.PlayHeadFrame);
        }
    }
}