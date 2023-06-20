using System;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline;

namespace FramePFX.Core.Automation.ViewModels {
    public class AutomationEngineViewModel {
        public AutomationEngine Model { get; }

        public ProjectViewModel Project { get; }

        public AutomationEngineViewModel(ProjectViewModel project, AutomationEngine model) {
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public void TickAndRefreshProject(bool isRendering) {
            this.TickAndRefreshProjectAtFrame(isRendering, this.Project.Timeline.PlayHeadFrame);
        }

        public void TickAndRefreshProjectAtFrame(bool isRendering, long frame) {
            this.Model.TickProjectAtFrame(frame);
            if (!isRendering) {
                this.RefreshTimeline(this.Project.Timeline, frame);
            }
        }

        public void RefreshTimeline(TimelineViewModel timeline, long frame) {
            timeline.IsAutomationChangeInProgress = true;
            try {
                foreach (AutomationSequenceViewModel sequence in timeline.AutomationData.Sequences) {
                    if (sequence.IsAutomationInUse) {
                        sequence.DoRefreshValue(this, frame, true);
                    }
                }
            }
            finally {
                timeline.IsAutomationChangeInProgress = false;
            }

            foreach (LayerViewModel layer in timeline.Layers) {
                this.RefreshLayer(layer, frame);
            }
        }

        public void RefreshLayer(LayerViewModel layer, long frame) {
            layer.IsAutomationChangeInProgress = true;
            try {
                foreach (AutomationSequenceViewModel sequence in layer.AutomationData.Sequences) {
                    if (sequence.IsAutomationInUse) {
                        sequence.DoRefreshValue(this, frame, true);
                    }
                }
            }
            finally {
                layer.IsAutomationChangeInProgress = false;
            }

            foreach (ClipViewModel clip in layer.Clips) {
                if (clip.IntersectsFrameAt(frame)) {
                    this.RefreshClip(clip, frame);
                }
            }
        }

        public void RefreshClip(ClipViewModel clip, long frame) {
            clip.IsAutomationChangeInProgress = true;
            try {
                foreach (AutomationSequenceViewModel sequence in clip.AutomationData.Sequences) {
                    if (sequence.IsAutomationInUse) {
                        sequence.DoRefreshValue(this, frame, true);
                    }
                }
            }
            finally {
                clip.IsAutomationChangeInProgress = false;
            }
        }

        public void OnOverrideStateChanged(AutomationDataViewModel data, AutomationSequenceViewModel sequence) {
            long frame = this.Project.Timeline.PlayHeadFrame;
            sequence.Model.DoUpdateValue(this.Model, frame);
            sequence.DoRefreshValue(this, frame, false);
        }
    }
}