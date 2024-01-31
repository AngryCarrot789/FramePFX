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

            // list.Add(new EventContextEntry(DeleteSelectedClips, selectedCount == 1 ? "Delete Clip" : "Delete Clips"));
            list.Add(new ActionContextEntry("actions.timeline.RenameClipAction", "Rename clip"));
            list.Add(new SeparatorEntry());
            list.Add(new ActionContextEntry("actions.timeline.DeleteSelectedClips", selectedCount == 1 ? "Delete Clip" : "Delete Clips", "Delete all selected clips in this timeline"));
            list.Add(new ActionContextEntry("actions.timeline.DeleteClipOwnerTrack", "Delete Track", "Deletes the track that this clip resides in"));
        }
    }
}