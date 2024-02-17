using FramePFX.CommandSystem;
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

        public override void Execute(CommandEventArgs e) {
            Timeline timeline;
            if (DataKeys.TrackKey.TryGetContext(e.DataContext, out Track track) && (timeline = track.Timeline) != null || DataKeys.TimelineKey.TryGetContext(e.DataContext, out timeline)) {
                SelectAll(timeline, track, false);
            }
        }
    }

    public class SelectAllClipsInTrackCommand : Command {
        public override bool CanExecute(CommandEventArgs e) {
            return e.DataContext.ContainsKey(DataKeys.TrackKey);
        }

        public override void Execute(CommandEventArgs e) {
            if (DataKeys.TrackKey.TryGetContext(e.DataContext, out Track track)) {
                track.SelectAll();
                VideoEditorPropertyEditor.Instance.UpdateClipSelectionAsync(track.Timeline);
            }
        }
    }

    public class SelectAllClipsInTimelineCommand : Command {
        public override bool CanExecute(CommandEventArgs e) {
            return e.DataContext.ContainsKey(DataKeys.TrackKey) || e.DataContext.ContainsKey(DataKeys.TimelineKey);
        }

        public override void Execute(CommandEventArgs e) {
            Timeline timeline;
            if (DataKeys.TrackKey.TryGetContext(e.DataContext, out Track track) && (timeline = track.Timeline) != null || DataKeys.TimelineKey.TryGetContext(e.DataContext, out timeline)) {
                SelectAllTracksCommand.SelectAll(timeline, track, true);
            }
        }
    }
}