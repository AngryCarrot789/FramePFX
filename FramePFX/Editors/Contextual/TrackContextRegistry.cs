using System.Collections.Generic;
using System.Linq;
using FramePFX.AdvancedContextService;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Contextual {
    public class TrackContextRegistry : IContextGenerator {
        public static TrackContextRegistry Instance { get; } = new TrackContextRegistry();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            Timeline timeline = null;
            if (context.TryGetContext(DataKeys.TrackKey, out Track track) && track.Timeline != null) {
                int selectedCount = track.Timeline.SelectedTracks.Count;
                if (!track.IsSelected)
                    selectedCount++;

                timeline = track.Timeline;
                list.Add(new EventContextEntry(DeleteSelectedTracks, selectedCount == 1 ? "Delete Track" : "Delete Tracks"));
            }

            if (timeline != null || context.TryGetContext(DataKeys.TimelineKey, out timeline)) {
                if (list.Count > 0) {
                    list.Add(SeparatorEntry.NewInstance);
                }

                list.Add(new EventContextEntry(AddVideoTrack, "New Video Track"));
            }
        }

        private static void AddVideoTrack(IDataContext context) {
            Timeline timeline;
            if (context.TryGetContext(DataKeys.TrackKey, out Track track) && (timeline = track.Timeline) != null || context.TryGetContext(DataKeys.TimelineKey, out timeline)) {
                timeline.AddTrack(new VideoTrack() {
                    DisplayName = "New Video Track"
                });
            }
        }

        private static void DeleteSelectedTracks(IDataContext context) {
            if (!context.TryGetContext(DataKeys.TrackKey, out Track track) || track.Timeline == null) {
                return;
            }

            foreach (Track theTrack in track.Timeline.SelectedTracks.ToList()) {
                theTrack.Timeline.DeleteTrack(theTrack);
            }

            if (track.Timeline != null)
                track.Timeline.DeleteTrack(track);
        }
    }
}