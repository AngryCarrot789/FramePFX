using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class DeleteTracksAction : AnAction {
        public override Task ExecuteAsync(AnActionEventArgs e) {
            int focusedIndex = -1;
            HashSet<Track> tracks = new HashSet<Track>();
            if (e.DataContext.TryGetContext(DataKeys.TrackKey, out Track focusedTrack)) {
                focusedIndex = focusedTrack.IndexInTimeline;
            }

            Timeline timeline;
            if ((timeline = focusedTrack?.Timeline) != null || e.DataContext.TryGetContext(DataKeys.TimelineKey, out timeline)) {
                foreach (Track track in timeline.SelectedTracks) {
                    tracks.Add(track);
                }
            }

            foreach (Track track in tracks) {
                track.Timeline.DeleteTrack(track);
            }

            focusedTrack?.Timeline?.DeleteTrack(focusedTrack);

            if (timeline != null && timeline.Tracks.Count > 0) {
                if (focusedIndex >= 0) {
                    if (focusedIndex >= timeline.Tracks.Count) {
                        timeline.Tracks[timeline.Tracks.Count - 1].SetIsSelected(true, true);
                    }
                    else {
                        timeline.Tracks[focusedIndex].SetIsSelected(true, true);
                    }
                }
                else {
                    timeline.Tracks[0].SetIsSelected(true, true);
                }
            }

            return Task.CompletedTask;
        }
    }
}