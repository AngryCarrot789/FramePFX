using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Utils;

namespace FramePFX.Editor.Actions {
    public class MoveFrameByFrameActions {
        [ActionRegistration("actions.timeline.surface.MoveBack")]
        public class MoveBackAction : ExecutableAction {
            public override Task<bool> ExecuteAsync(ActionEventArgs e) => ExecuteGeneral(e, -1);
        }

        [ActionRegistration("actions.timeline.surface.MoveForward")]
        public class MoveForwardAction : ExecutableAction {
            public override Task<bool> ExecuteAsync(ActionEventArgs e) => ExecuteGeneral(e, 1);
        }

        [ActionRegistration("actions.timeline.surface.ExpandEndBackwards")]
        public class ExpandEndBackwardsAction : ExecutableAction {
            public override Task<bool> ExecuteAsync(ActionEventArgs e) => ExecuteGeneral(e, -1, true);
        }

        [ActionRegistration("actions.timeline.surface.ExpandEndForward")]
        public class ExpandEndForwardAction : ExecutableAction {
            public override Task<bool> ExecuteAsync(ActionEventArgs e) => ExecuteGeneral(e, 1, true);
        }

        [ActionRegistration("actions.timeline.surface.ExpandBeginBack")]
        public class ExpandBeginBackAction : ExecutableAction {
            public override Task<bool> ExecuteAsync(ActionEventArgs e) => ExecuteGeneral(e, -1, true, true);
        }

        [ActionRegistration("actions.timeline.surface.ExpandBeginForward")]
        public class ExpandBeginForwardAction : ExecutableAction {
            public override Task<bool> ExecuteAsync(ActionEventArgs e) => ExecuteGeneral(e, 1, true, true);
        }

        [ActionRegistration("actions.timeline.FrameBack")]
        public class TimelineFrameBackAction : ExecutableAction {
            public override Task<bool> ExecuteAsync(ActionEventArgs e) => ExecuteTimeline(e, -1, false);
        }

        [ActionRegistration("actions.timeline.FrameForward")]
        public class TimelineFrameForwardAction : ExecutableAction {
            public override Task<bool> ExecuteAsync(ActionEventArgs e) => ExecuteTimeline(e, 1, false);
        }

        public static Task<bool> ExecuteGeneral(ActionEventArgs e, long amount, bool expandMode = false, bool resizeBothWays = false, bool forcePlayHead = false) {
            if (e.DataContext.TryGetContext(out ClipViewModel clip) && clip.Timeline != null) {
                OnClipAction(clip, amount, expandMode, resizeBothWays, forcePlayHead);
            }
            else if (e.DataContext.TryGetContext(out TrackViewModel track) && track.Timeline != null) {
                OnTimelineAction(track.Timeline, amount);
            }
            else if (e.DataContext.TryGetContext(out TimelineViewModel timeline)) {
                OnTimelineAction(timeline, amount);
            }
            else {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public static Task<bool> ExecuteTimeline(ActionEventArgs e, long amount, bool canMultiplyZoom = true) {
            TimelineViewModel timeline;
            if (e.DataContext.TryGetContext(out ClipViewModel clip) && (timeline = clip.Timeline) != null) {
                OnTimelineAction(timeline, amount, canMultiplyZoom);
            }
            else if (e.DataContext.TryGetContext(out TrackViewModel track) && (timeline = track.Timeline) != null) {
                OnTimelineAction(timeline, amount, canMultiplyZoom);
            }
            else if (e.DataContext.TryGetContext(out timeline)) {
                OnTimelineAction(timeline, amount, canMultiplyZoom);
            }
            else {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        // expandMode: holding shift
        // resizeBothWays: holding ctrl + shift
        public static void OnClipAction(ClipViewModel clip, long frame, bool expandMode, bool resizeBothWays, bool forcePlayHead) {
            TimelineViewModel timeline = clip.Timeline;
            if (forcePlayHead) {
                OnTimelineAction(timeline, frame);
                return;
            }

            frame = GetZoomMultiplied(frame, timeline.UnitZoom);
            FrameSpan span = clip.FrameSpan;
            if (expandMode && !resizeBothWays) {
                long duration = span.Duration + frame;
                if (duration < 1) {
                    return;
                }

                if ((span.Begin + duration) > timeline.MaxDuration) {
                    timeline.MaxDuration += Maths.Clamp(frame + 500, 0, 500);
                }

                clip.FrameDuration = duration;
            }
            else {
                long begin = span.Begin + frame;
                if (begin < 0) {
                    if ((begin = 0) == span.Begin) {
                        return;
                    }
                }
                else if (begin > timeline.MaxDuration) {
                    timeline.MaxDuration += Maths.Clamp(frame + 500, 0, 500);
                }
                else if (resizeBothWays && begin >= span.EndIndex) {
                    return;
                }

                clip.FrameSpan = resizeBothWays ? span.WithBeginIndex(begin) : span.WithBegin(begin);
            }
        }

        public static void OnTimelineAction(TimelineViewModel timeline, long frame, bool canMultiplyZoom = true) {
            if (canMultiplyZoom) {
                frame = GetZoomMultiplied(frame, timeline.UnitZoom);
            }

            long duration = timeline.PlayHeadFrame + frame;
            if (duration < 0) {
                return;
            }

            if ((timeline.PlayHeadFrame + duration) > timeline.MaxDuration) {
                timeline.MaxDuration = ((timeline.PlayHeadFrame + duration) - timeline.MaxDuration) + 500;
            }

            timeline.PlayHeadFrame = duration;
        }

        public static long GetZoomMultiplied(long amount, double zoom) {
            if (zoom < 0.1d) {
                return amount * 25;
            }
            else if (zoom < 0.5) {
                return amount * 10;
            }
            else if (zoom < 1) {
                return amount * 5;
            }
            else {
                return amount;
            }
        }
    }
}