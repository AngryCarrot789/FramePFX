using System.Collections.Generic;
using FramePFX.AdvancedContextService;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Contextual {
    public class ClipContextRegistry : IContextGenerator {
        public static ClipContextRegistry Instance { get; } = new ClipContextRegistry();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (!context.TryGetContext(DataKeys.ClipKey, out Clip clip)) {
                return;
            }

            int selectedCount = clip.Timeline.GetSelectedClipCountWith(clip);

            list.Add(new EventContextEntry(DeleteSelectedClips, selectedCount == 1 ? "Delete Clip" : "Delete Clips"));
            list.Add(SeparatorEntry.NewInstance);
            list.Add(new EventContextEntry(DeleteTrackClipIsIn, "Delete Track"));
        }

        private static void DeleteSelectedClips(IDataContext context) {
            if (!context.TryGetContext(DataKeys.ClipKey, out Clip clip) || clip.Timeline == null) {
                return;
            }

            foreach (Clip theClip in clip.Timeline.GetSelectedClipsWith(clip)) {
                theClip.Destroy();
                theClip.Track.RemoveClip(theClip);
            }
        }

        private static void DeleteTrackClipIsIn(IDataContext context) {
            if (!context.TryGetContext(DataKeys.ClipKey, out Clip clip)) {
                return;
            }

            clip.Timeline?.DeleteTrack(clip.Track);
        }
    }
}