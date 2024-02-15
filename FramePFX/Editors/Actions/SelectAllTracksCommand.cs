using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;
using FramePFX.PropertyEditing;

namespace FramePFX.Editors.Actions {
    public class SelectAllTracksCommand : Command {
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

            VideoEditorPropertyEditor.Instance.UpdateTrackSelectionAsync(timeline);
        }

        public override bool CanExecute(CommandEventArgs e) {
            return e.DataContext.ContainsKey(DataKeys.TimelineKey) || e.DataContext.ContainsKey(DataKeys.TrackKey);
        }

        public override Task ExecuteAsync(CommandEventArgs e) {
            Timeline timeline;
            if (e.DataContext.TryGetContext(DataKeys.TrackKey, out Track track) && (timeline = track.Timeline) != null || e.DataContext.TryGetContext(DataKeys.TimelineKey, out timeline)) {
                SelectAll(timeline, track, false);
            }

            return Task.CompletedTask;
        }
    }

    public class SelectAllClipsInTrackCommand : Command {
        public override bool CanExecute(CommandEventArgs e) {
            return e.DataContext.ContainsKey(DataKeys.TrackKey);
        }

        public override Task ExecuteAsync(CommandEventArgs e) {
            if (e.DataContext.TryGetContext(DataKeys.TrackKey, out Track track)) {
                track.SelectAll();
                VideoEditorPropertyEditor.Instance.UpdateClipSelectionAsync(track.Timeline);
            }

            return Task.CompletedTask;
        }
    }

    public class SelectAllClipsInTimelineCommand : Command {
        public override bool CanExecute(CommandEventArgs e) {
            return e.DataContext.ContainsKey(DataKeys.TrackKey) || e.DataContext.ContainsKey(DataKeys.TimelineKey);
        }

        public override Task ExecuteAsync(CommandEventArgs e) {
            Timeline timeline;
            if (e.DataContext.TryGetContext(DataKeys.TrackKey, out Track track) && (timeline = track.Timeline) != null || e.DataContext.TryGetContext(DataKeys.TimelineKey, out timeline)) {
                SelectAllTracksCommand.SelectAll(timeline, track, true);
            }

            return Task.CompletedTask;
        }
    }
}