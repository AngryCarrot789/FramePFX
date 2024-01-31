using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class SelectAllTracksAction : AnAction {
        public static void SelectAll(Timeline timeline, Track focusedTrack, bool selectClipsToo = false) {
            foreach (Track t in timeline.Tracks) {
                if (selectClipsToo) {
                    t.SelectAll();
                }

                t.SetIsSelected(true);
            }

            if (focusedTrack != null) {
                focusedTrack.SetIsSelected(true, true);
            }
            else if (timeline.Tracks.Count > 0) {
                timeline.Tracks[timeline.Tracks.Count - 1].SetIsSelected(true, true);
            }
        }

        public override bool CanExecute(AnActionEventArgs e) {
            return e.DataContext.ContainsKey(DataKeys.TimelineKey) || e.DataContext.ContainsKey(DataKeys.TrackKey);
        }

        public override Task ExecuteAsync(AnActionEventArgs e) {
            Timeline timeline;
            if (e.DataContext.TryGetContext(DataKeys.TrackKey, out Track track) && (timeline = track.Timeline) != null || e.DataContext.TryGetContext(DataKeys.TimelineKey, out timeline)) {
                SelectAll(timeline, track, false);
            }

            return Task.CompletedTask;
        }
    }

    public class SelectAllClipsInTrackAction : AnAction {
        public override bool CanExecute(AnActionEventArgs e) {
            return e.DataContext.ContainsKey(DataKeys.TrackKey);
        }

        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (e.DataContext.TryGetContext(DataKeys.TrackKey, out Track track)) {
                track.SelectAll();
            }

            return Task.CompletedTask;
        }
    }

    public class SelectAllClipsInTimelineAction : AnAction {
        public override bool CanExecute(AnActionEventArgs e) {
            return e.DataContext.ContainsKey(DataKeys.TrackKey) || e.DataContext.ContainsKey(DataKeys.TimelineKey);
        }

        public override Task ExecuteAsync(AnActionEventArgs e) {
            Timeline timeline;
            if (e.DataContext.TryGetContext(DataKeys.TrackKey, out Track track) && (timeline = track.Timeline) != null || e.DataContext.TryGetContext(DataKeys.TimelineKey, out timeline)) {
                SelectAllTracksAction.SelectAll(timeline, track, true);
            }

            return Task.CompletedTask;
        }
    }
}