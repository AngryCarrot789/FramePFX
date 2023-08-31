using System;
using System.Collections.ObjectModel;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Editor.Timelines.Effects.ViewModels;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Utils;

namespace FramePFX.Automation.ViewModels {
    public class AutomationEngineViewModel {
        public AutomationEngine Model { get; }

        public ProjectViewModel Project { get; }

        public bool IsPlayback => this.Project.Editor?.Playback.IsPlaying ?? false;

        public AutomationEngineViewModel(ProjectViewModel project, AutomationEngine model) {
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public void UpdateAndRefresh(bool refresh) {
            this.UpdateAndRefreshAt(refresh, this.Project.Timeline.PlayHeadFrame);
        }

        public void UpdateAndRefreshAt(bool refresh, long frame) {
            this.Model.UpdateAt(frame);
            if (refresh) {
                this.RefreshTimeline(this.Project.Timeline, frame);
            }
        }

        public void RefreshTimeline(TimelineViewModel timeline, long frame) {
            ReadOnlyObservableCollection<AutomationSequenceViewModel> sequences = timeline.AutomationData.Sequences;
            try {
                timeline.IsAutomationChangeInProgress = true;
                for (int i = 0, c = sequences.Count; i < c; i++) {
                    AutomationSequenceViewModel sequence = sequences[i];
                    if (sequence.IsAutomationInUse) {
                        sequence.DoRefreshValue(this, frame, true, true);
                    }
                }

                for (int i = 0, c = timeline.Tracks.Count; i < c; i++) {
                    TrackViewModel track = timeline.Tracks[i];
                    this.RefreshTrack(track, frame);
                }
            }
            finally {
                timeline.IsAutomationChangeInProgress = false;
            }
        }

        public void RefreshTrack(TrackViewModel track, long frame) {
            // this is super messy but it's basically just caching local variables, to just barely outperform enumerators
            int i, c, j, k;
            AutomationSequenceViewModel sequence;
            ReadOnlyObservableCollection<AutomationSequenceViewModel> sequences;
            try {
                sequences = track.AutomationData.Sequences;
                track.IsAutomationRefreshInProgress = true;
                for (i = 0, c = sequences.Count; i < c; i++) {
                    sequence = sequences[i];
                    if (sequence.IsAutomationInUse) {
                        sequence.DoRefreshValue(this, frame, true, true);
                    }
                }

                ReadOnlyObservableCollection<ClipViewModel> clips = track.Clips;
                for (i = 0, c = clips.Count; i < c; i++) {
                    ClipViewModel clip = track.Clips[i];
                    FrameSpan span = clip.FrameSpan;
                    long relativeFrame = frame - span.Begin;
                    if (relativeFrame >= 0 && relativeFrame < span.Duration) {
                        try {
                            sequences = clip.AutomationData.Sequences;
                            clip.IsAutomationRefreshInProgress = true;
                            for (j = 0, k = sequences.Count; j < k; j++) {
                                sequence = sequences[j];
                                if (sequence.IsAutomationInUse) {
                                    sequence.DoRefreshValue(this, relativeFrame, true, true);
                                }
                            }
                        }
                        finally {
                            clip.IsAutomationRefreshInProgress = false;
                        }

                        this.UpdateClipEffects(clip, relativeFrame);
                    }
                }
            }
            finally {
                track.IsAutomationRefreshInProgress = false;
            }
        }

        private void UpdateClipEffects(ClipViewModel clip, long frame) {
            foreach (BaseEffectViewModel effect in clip.Effects) {
                try {
                    ReadOnlyObservableCollection<AutomationSequenceViewModel> seq = effect.AutomationData.Sequences;
                    effect.IsAutomationRefreshInProgress = true;
                    for (int j = 0, k = seq.Count; j < k; j++) {
                        AutomationSequenceViewModel sequence = seq[j];
                        if (sequence.IsAutomationInUse) {
                            sequence.DoRefreshValue(this, frame, true, true);
                        }
                    }
                }
                finally {
                    effect.IsAutomationRefreshInProgress = false;
                }
            }
        }

        public void OnOverrideStateChanged(AutomationDataViewModel data, AutomationSequenceViewModel sequence) {
            long frame = this.Project.Timeline.PlayHeadFrame;
            if (data.Owner is ClipViewModel clip) {
                FrameSpan span = clip.FrameSpan;
                frame = Maths.Clamp(frame - span.Begin, 0, span.Duration - 1);
            }

            if (sequence.IsOverrideEnabled) {
                sequence.OverrideKeyFrame.Model.AssignCurrentValue(frame, sequence.Model);
            }

            sequence.Model.DoUpdateValue(frame);
            sequence.DoRefreshValue(this, frame, this.IsPlayback, false);
        }

        public void OnKeyFrameChanged(AutomationDataViewModel data, AutomationSequenceViewModel sequence, KeyFrameViewModel keyFrame) {
            long frame = this.Project.Timeline.PlayHeadFrame;
            if (data.Owner is ClipViewModel clip) {
                FrameSpan span = clip.FrameSpan;
                frame = Maths.Clamp(frame - span.Begin, 0, span.Duration - 1);
            }

            sequence.Model.DoUpdateValue(frame);
            sequence.DoRefreshValue(this, frame, this.IsPlayback, false);
        }
    }
}