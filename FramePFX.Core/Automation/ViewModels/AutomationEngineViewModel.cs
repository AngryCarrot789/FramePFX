using System;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timelines;

namespace FramePFX.Core.Automation.ViewModels {
    public class AutomationEngineViewModel {
        public AutomationEngine Model { get; }

        public ProjectViewModel Project { get; }

        public bool IsPlayback => this.Project.Editor?.Playback.IsPlaying ?? false;

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

        private void RefreshTimeline(TimelineViewModel timeline, long frame) {
            timeline.IsAutomationChangeInProgress = true;
            try {
                foreach (AutomationSequenceViewModel sequence in timeline.AutomationData.Sequences) {
                    if (sequence.IsAutomationInUse) {
                        sequence.DoRefreshValue(this, frame, true, true);
                    }
                }
            }
            finally {
                timeline.IsAutomationChangeInProgress = false;
            }

            foreach (TrackViewModel track in timeline.Tracks) {
                this.RefreshTrack(track, frame);
            }
        }

        private void RefreshTrack(TrackViewModel track, long frame) {
            track.IsAutomationRefreshInProgress = true;
            try {
                foreach (AutomationSequenceViewModel sequence in track.AutomationData.Sequences) {
                    if (sequence.IsAutomationInUse) {
                        sequence.DoRefreshValue(this, frame, true, true);
                    }
                }
            }
            finally {
                track.IsAutomationRefreshInProgress = false;
            }

            foreach (ClipViewModel clip in track.Clips) {
                if (clip.IntersectsFrameAt(frame)) {
                    this.RefreshClip(clip, frame);
                }
            }
        }

        private void RefreshClip(ClipViewModel clip, long absoluteFrame) {
            clip.IsAutomationRefreshInProgress = true;
            try {
                long relativeFrame = ((IAutomatable) clip.Model).GetRelativeFrame(absoluteFrame);
                foreach (AutomationSequenceViewModel sequence in clip.AutomationData.Sequences) {
                    if (sequence.IsAutomationInUse) {
                        sequence.DoRefreshValue(this, relativeFrame, true, true);
                    }
                }
            }
            finally {
                clip.IsAutomationRefreshInProgress = false;
            }
        }

        public void OnOverrideStateChanged(AutomationDataViewModel data, AutomationSequenceViewModel sequence) {
            long frame = data.Owner.AutomationModel.GetRelativeFrame(this.Project.Timeline.PlayHeadFrame);
            sequence.Model.DoUpdateValue(this.Model, frame);
            sequence.DoRefreshValue(this, frame, this.IsPlayback, false);
        }

        public void OnKeyFrameChanged(AutomationDataViewModel data, AutomationSequenceViewModel sequence, KeyFrameViewModel keyFrame) {
            long frame = data.Owner.AutomationModel.GetRelativeFrame(this.Project.Timeline.PlayHeadFrame);
            sequence.Model.DoUpdateValue(this.Model, frame);
            sequence.DoRefreshValue(this, frame, this.IsPlayback, false);
        }
    }
}