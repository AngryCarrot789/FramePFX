using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class DeleteTracksAction : AnAction {
        public override Task ExecuteAsync(AnActionEventArgs e) {
            HashSet<Track> tracks = new HashSet<Track>();
            if (e.DataContext.TryGetContext(DataKeys.TrackKey, out Track focusedTrack)) {
                tracks.Add(focusedTrack);
            }

            Timeline timeline;
            if ((timeline = focusedTrack.Timeline) != null || e.DataContext.TryGetContext(DataKeys.TimelineKey, out timeline)) {
                foreach (Track track in timeline.SelectedTracks) {
                    tracks.Add(track);
                }
            }

            foreach (Track track in tracks) {
                track.Destroy();
                track.Timeline.RemoveTrack(track);
            }

            return Task.CompletedTask;
        }
    }
}