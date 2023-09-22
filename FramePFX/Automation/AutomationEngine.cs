using System;
using System.Collections.Generic;
using FramePFX.Automation.Events;
using FramePFX.Automation.Keyframe;
using FramePFX.Automation.ViewModels;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.Timelines.Effects;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Effects;
using FramePFX.Utils;

namespace FramePFX.Automation {
    public static class AutomationEngine {
        public static void UpdateTimeline(Timeline timeline, long frame) {
            UpdateAutomationData(timeline, frame);
            foreach (Track track in timeline.Tracks) {
                UpdateAutomationData(track, frame);
                IReadOnlyList<Clip> clips = track.Clips;
                for (int i = clips.Count - 1; i >= 0; i--) {
                    Clip clip = clips[i];
                    if (clip.GetRelativeFrame(frame, out long relative)) {
                        UpdateAutomationData(clip, relative);
                        foreach (BaseEffect effect in clip.Effects) {
                            UpdateAutomationData(effect, relative);
                        }

                        if (clip is CompositionVideoClip composition && composition.ResourceHelper.TryGetResource(out ResourceCompositionSeq resource)) {
                            UpdateTimeline(resource.Timeline, relative);
                        }
                    }
                }
            }
        }

        private static void UpdateAutomationData(IAutomatable automatable, long frame) {
            try {
                automatable.IsAutomationChangeInProgress = true;
                foreach (AutomationSequence sequence1 in automatable.AutomationData.Sequences) {
                    if (sequence1.IsAutomationReady) {
                        sequence1.DoUpdateValue(frame);
                    }
                }
            }
            finally {
                automatable.IsAutomationChangeInProgress = false;
            }
        }

        public static void UpdateAndRefreshTimeline(TimelineViewModel timeline, long frame) {
            UpdateTimeline(timeline.Model, frame);
            RefreshTimeline(timeline, frame);
        }

        public static void RefreshTimeline(TimelineViewModel timeline, long frame) {
            RefreshAutomationValueEventArgs args1 = new RefreshAutomationValueEventArgs(frame, true, true);
            RefreshSequences(timeline, in args1);
            foreach (TrackViewModel track in timeline.Tracks) {
                RefreshSequences(track, in args1);
                foreach (ClipViewModel clip in track.Clips) {
                    FrameSpan span = clip.FrameSpan;
                    long relative = frame - span.Begin;
                    if (relative >= 0 && relative < span.Duration) {
                        RefreshAutomationValueEventArgs args2 = new RefreshAutomationValueEventArgs(relative, true, true);
                        RefreshSequences(clip, in args2);
                        foreach (BaseEffectViewModel effect in clip.Effects) {
                            RefreshSequences(effect, in args2);
                        }
                    }
                }
            }
        }

        private static void RefreshSequences(IAutomatableViewModel automatable, in RefreshAutomationValueEventArgs e) {
            try {
                automatable.IsAutomationRefreshInProgress = true;
                foreach (AutomationSequenceViewModel sequence in automatable.AutomationData.Sequences) {
                    if (sequence.IsAutomationReady) {
                        sequence.DoRefreshValue(e);
                    }
                }
            }
            finally {
                automatable.IsAutomationRefreshInProgress = false;
            }
        }

        public static void OnOverrideStateChanged(AutomationDataViewModel data, AutomationSequenceViewModel sequence) {
            TimelineViewModel timeline = data.Owner.Timeline;
            if (timeline == null) {
                throw new Exception("No timeline associated with automation data owner: " + data.Owner);
            }

            long frame = timeline.PlayHeadFrame;
            if (data.Owner is ClipViewModel clip) {
                FrameSpan span = clip.FrameSpan;
                frame = Maths.Clamp(frame - span.Begin, 0, span.Duration - 1);
            }

            if (sequence.IsOverrideEnabled) {
                sequence.OverrideKeyFrame.Model.AssignCurrentValue(frame, sequence.Model);
            }

            sequence.Model.DoUpdateValue(frame);
            sequence.DoRefreshValue(frame, timeline.Project.Editor.Playback.IsPlaying, false);
        }

        public static void OnKeyFrameChanged(AutomationDataViewModel data, AutomationSequenceViewModel sequence, KeyFrameViewModel keyFrame) {
            TimelineViewModel timeline = data.Owner.Timeline;
            if (timeline == null) {
                throw new Exception("No timeline associated with automation data owner: " + data.Owner);
            }

            long frame = timeline.PlayHeadFrame;
            if (data.Owner is ClipViewModel clip) {
                FrameSpan span = clip.FrameSpan;
                frame = Maths.Clamp(frame - span.Begin, 0, span.Duration - 1);
            }

            sequence.Model.DoUpdateValue(frame);
            sequence.DoRefreshValue(frame, timeline.Project.Editor.Playback.IsPlaying, false);
        }
    }
}