using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.Timelines;

namespace FramePFX.Core.Automation {
    public class AutomationEngine {
        public Project Project { get; }

        public AutomationEngine(Project project) {
            this.Project = project;
        }

        public void TickProject() {
            this.TickProjectAtFrame(this.Project.Timeline.PlayHeadFrame);
        }

        public void TickProjectAtFrame(long frame) {
            Timeline timeline = this.Project.Timeline;
            this.UpdateTimeline(timeline, frame);
        }

        public void UpdateTimeline(Timeline timeline, long frame) {
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

            foreach (Track track in timeline.Tracks) {
                this.UpdateTrack(track, frame);
            }
        }

        public void UpdateTrack(Track track, long frame) {
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

            foreach (Clip clip in track.Clips) {
                if (clip.IntersectsFrameAt(frame)) {
                    this.UpdateClip(clip, frame);
                }
            }
        }

        public void UpdateClip(Clip clip, long absoluteFrame) {
            clip.IsAutomationChangeInProgress = true;
            try {
                long relativeFrame = ((IAutomatable) clip).GetRelativeFrame(absoluteFrame);
                foreach (AutomationSequence sequence in clip.AutomationData.Sequences) {
                    if (sequence.IsAutomationInUse) {
                        sequence.DoUpdateValue(this, relativeFrame);
                    }
                }
            }
            finally {
                clip.IsAutomationChangeInProgress = false;
            }
        }
    }
}