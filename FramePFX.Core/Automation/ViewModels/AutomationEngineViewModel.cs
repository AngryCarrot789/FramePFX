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
            layer.IsAutomationRefreshInProgress = true;
            try {
                foreach (AutomationSequenceViewModel sequence in layer.AutomationData.Sequences) {
                    if (sequence.IsAutomationInUse) {
                        sequence.DoRefreshValue(this, frame, true);
                    }
                }
            }
            finally {
                layer.IsAutomationRefreshInProgress = false;
            }

            foreach (ClipViewModel clip in layer.Clips) {
                if (clip.IntersectsFrameAt(frame)) {
                    this.RefreshClip(clip, frame);
                }
            }
        }

        public void RefreshClip(ClipViewModel clip, long frame) {
            clip.IsAutomationRefreshInProgress = true;
            try {
                long offset = frame - clip.FrameBegin;
                if (offset < 0 || frame >= clip.FrameEndIndex) {
                    throw new Exception($"Clip it not within the range of the given frame: {clip.FrameSpan} does not contain {frame}");
                }

                foreach (AutomationSequenceViewModel sequence in clip.AutomationData.Sequences) {
                    if (sequence.IsAutomationInUse) {
                        sequence.DoRefreshValue(this, offset, true);
                    }
                }
            }
            finally {
                clip.IsAutomationRefreshInProgress = false;
            }
        }

        public void OnOverrideStateChanged(AutomationDataViewModel data, AutomationSequenceViewModel sequence) {
            long frame = this.Project.Timeline.PlayHeadFrame;
            sequence.Model.DoUpdateValue(this.Model, frame);
            sequence.DoRefreshValue(this, frame, false);
        }

        public void OnKeyFrameValueChanged(AutomationDataViewModel data, AutomationSequenceViewModel sequence, KeyFrameViewModel keyFrame) {
            long frame = this.Project.Timeline.PlayHeadFrame;
            sequence.Model.DoUpdateValue(this.Model, frame);
            sequence.DoRefreshValue(this, frame, false);
        }
    }
}