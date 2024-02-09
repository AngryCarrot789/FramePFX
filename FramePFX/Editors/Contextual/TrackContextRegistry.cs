using System.Collections.Generic;
using FramePFX.AdvancedContextService;
using FramePFX.Editors.Factories;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;
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
                list.Add(new ActionContextEntry("actions.timeline.RenameTrackAction", "Rename track"));
                list.Add(new SeparatorEntry());
                list.Add(new EventContextEntry((c) => AddClipByType(c, ClipFactory.Instance.GetId(typeof(ImageVideoClip))), "Add Image Clip"));
                list.Add(new EventContextEntry((c) => AddClipByType(c, ClipFactory.Instance.GetId(typeof(TimecodeClip))), "Add Timecode Clip"));
                list.Add(new SeparatorEntry());
                list.Add(new ActionContextEntry("actions.timeline.DeleteSelectedTracks", selectedCount == 1 ? "Delete Track" : "Delete Tracks", "Delete selected tracks in this timeline"));
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

        private static void AddClipByType(IDataContext context, string id) {
            if (!context.TryGetContext(DataKeys.TrackKey, out Track track)) {
                return;
            }

            FrameSpan span = new FrameSpan(0, 300);
            if (context.TryGetContext(DataKeys.TrackContextMouseFrameKey, out long frame)) {
                span = span.WithBegin(frame);
            }

            Clip clip = ClipFactory.Instance.NewClip(id);
            clip.FrameSpan = span;
            track.AddClip(clip);
        }
    }
}