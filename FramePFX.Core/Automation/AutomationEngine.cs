using System;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.Timeline;

namespace FramePFX.Core.Automation {
    public class AutomationEngine {
        public ProjectModel Project { get; }

        public AutomationEngine(ProjectModel project) {
            this.Project = project;
        }

        public void TickProject() {
            this.TickProjectAtFrame(this.Project.Timeline.PlayHeadFrame);
        }

        public void TickProjectAtFrame(long frame) {
            TimelineModel timeline = this.Project.Timeline;
            this.UpdateTimeline(timeline, frame);
        }

        public void UpdateTimeline(TimelineModel timeline, long frame) {
            timeline.IsAutomationChangeInProgress = true;
            try {
                foreach (AutomationSequence sequence in timeline.AutomationData.Sequences) {
                    if (sequence.IsAutomationInUse) {
                        sequence.DoUpdateValue(this, frame);
                    }
                }
            }
            finally {
                timeline.IsAutomationChangeInProgress = false;
            }

            foreach (TrackModel track in timeline.Tracks) {
                if (track.CanUpdateAutomation()) {
                    this.UpdateTrack(track, frame);
                }
            }
        }

        public void UpdateTrack(TrackModel track, long frame) {
            track.IsAutomationChangeInProgress = true;
            try {
                foreach (AutomationSequence sequence in track.AutomationData.Sequences) {
                    if (sequence.IsAutomationInUse) {
                        sequence.DoUpdateValue(this, frame);
                    }
                }
            }
            finally {
                track.IsAutomationChangeInProgress = false;
            }

            foreach (ClipModel clip in track.Clips) {
                if (clip.IntersectsFrameAt(frame)) {
                    this.UpdateClip(clip, frame);
                }
            }
        }

        public void UpdateClip(ClipModel clip, long frame) {
            clip.IsAutomationChangeInProgress = true;
            try {
                long offset = frame - clip.FrameBegin;
                if (offset < 0 || frame >= clip.FrameEndIndex) {
                    throw new Exception($"Clip it not within the range of the given frame: {clip.FrameSpan} does not contain {frame}");
                }

                foreach (AutomationSequence sequence in clip.AutomationData.Sequences) {
                    if (sequence.IsAutomationInUse) {
                        sequence.DoUpdateValue(this, offset);
                    }
                }
            }
            finally {
                clip.IsAutomationChangeInProgress = false;
            }
        }
    }
}