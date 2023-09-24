using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Utils;

namespace FramePFX.Editor.Actions {
    public class MoveFrameByFrameActions {
        [ActionRegistration("actions.timeline.surface.MoveBack")]
        public class MoveBackAction : AnAction {
            public override Task<bool> ExecuteAsync(AnActionEventArgs e) => ExecuteGeneral(e, -1);
        }

        [ActionRegistration("actions.timeline.surface.MoveForward")]
        public class MoveForwardAction : AnAction {
            public override Task<bool> ExecuteAsync(AnActionEventArgs e) => ExecuteGeneral(e, 1);
        }

        [ActionRegistration("actions.timeline.surface.ExpandBack")]
        public class ExpandBackAction : AnAction {
            public override Task<bool> ExecuteAsync(AnActionEventArgs e) => ExecuteGeneral(e, -1, true);
        }

        [ActionRegistration("actions.timeline.surface.ExpandForward")]
        public class ExpandForwardAction : AnAction {
            public override Task<bool> ExecuteAsync(AnActionEventArgs e) => ExecuteGeneral(e, 1, true);
        }

        [ActionRegistration("actions.timeline.surface.SpecialExpandBack")]
        public class SpecialExpandBackAction : AnAction {
            public override Task<bool> ExecuteAsync(AnActionEventArgs e) => ExecuteGeneral(e, -1, true, true);
        }

        [ActionRegistration("actions.timeline.surface.SpecialExpandForward")]
        public class SpecialExpandForwardAction : AnAction {
            public override Task<bool> ExecuteAsync(AnActionEventArgs e) => ExecuteGeneral(e, 1, true, true);
        }

        [ActionRegistration("actions.timeline.FrameBack")]
        public class TimelineFrameBackAction : AnAction {
            public override Task<bool> ExecuteAsync(AnActionEventArgs e) => ExecuteTimeline(e, -1);
        }

        [ActionRegistration("actions.timeline.FrameForward")]
        public class TimelineFrameForwardAction : AnAction {
            public override Task<bool> ExecuteAsync(AnActionEventArgs e) => ExecuteTimeline(e, 1);
        }

        public static Task<bool> ExecuteGeneral(AnActionEventArgs e, long amount, bool expandMode = false, bool resizeBothWays = false) {
            if (e.DataContext.TryGetContext(out ClipViewModel clip) && clip.Timeline != null) {
                OnClipAction(clip, amount, expandMode, resizeBothWays);
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

        public static Task<bool> ExecuteTimeline(AnActionEventArgs e, long amount) {
            TimelineViewModel timeline;
            if (e.DataContext.TryGetContext(out ClipViewModel clip) && (timeline = clip.Timeline) != null) {
                OnTimelineAction(timeline, amount);
            }
            else if (e.DataContext.TryGetContext(out TrackViewModel track) && (timeline = track.Timeline) != null) {
                OnTimelineAction(timeline, amount);
            }
            else if (e.DataContext.TryGetContext(out timeline)) {
                OnTimelineAction(timeline, amount);
            }
            else {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        // expandMode: holding shift
        // resizeBothWays: holding ctrl + shift
        public static void OnClipAction(ClipViewModel clip, long frame, bool expandMode, bool resizeBothWays) {
            TimelineViewModel timeline = clip.Timeline;
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

        public static void OnTimelineAction(TimelineViewModel timeline, long frame) {
            frame = GetZoomMultiplied(frame, timeline.UnitZoom);
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
            if (zoom < 0.25d) {
                return amount * 25;
            }
            else if (zoom < 2) {
                return amount * 10;
            }
            else if (zoom < 5) {
                return amount * 5;
            }
            else {
                return amount;
            }
        }
    }
}