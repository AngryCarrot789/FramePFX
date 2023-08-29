using FramePFX.Automation.Keyframe;
using FramePFX.Editor;
using FramePFX.Editor.Timelines;
using FramePFX.Utils;

namespace FramePFX.Automation {
    public class AutomationEngine {
        public const long FrameRate = 1000L;

        public Project Project { get; }

        public AutomationEngine(Project project) {
            this.Project = project;
        }

        // public void UpdateAt(long frame, Rational timeBase) {
        //     // Timeline timeline = this.Project.Timeline;
        //     // this.UpdateTimeline(timeline, frame);
        //     Rational time = Timecode.FromFrame(this.Project.Settings.TimeBase, frame);
        //     Rational frameTime = Rational.Transform(time, this.Project.Settings.TimeBase, timeBase);
        //     this.UpdateAt(frameTime.ToInt);
        // }

        public void UpdateAt(long frame) {
            this.UpdateTimeline(this.Project.Timeline, frame);
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
                FrameSpan span = clip.FrameSpan;
                long relative = frame - span.Begin;
                if (relative >= 0 && relative < span.Duration) {
                    try {
                        clip.IsAutomationChangeInProgress = true;
                        foreach (AutomationSequence sequence in clip.AutomationData.Sequences) {
                            if (sequence.IsAutomationInUse) {
                                sequence.DoUpdateValue(this, relative);
                            }
                        }
                    }
                    finally {
                        clip.IsAutomationChangeInProgress = false;
                    }
                }
            }
        }
    }
}