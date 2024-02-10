using System.Collections.Generic;
using FramePFX.AdvancedContextService;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Clips.Core;
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
            if (clip is ICompositionClip)
                list.Add(new EventContextEntry(OpenClipTimeline, "Open Composition Timeline"));
            list.Add(new SeparatorEntry());
            list.Add(new ActionContextEntry("actions.timeline.DeleteSelectedClips", selectedCount == 1 ? "Delete Clip" : "Delete Clips", "Delete all selected clips in this timeline"));
            list.Add(new ActionContextEntry("actions.timeline.DeleteClipOwnerTrack", "Delete Track", "Deletes the track that this clip resides in"));
        }

        private static void OpenClipTimeline(IDataContext obj) {
            if (obj.TryGetContext(DataKeys.ClipKey, out Clip clip) && clip is ICompositionClip compositionClip) {
                if (clip.Project is Project project && compositionClip.ResourceCompositionKey.TryGetResource(out ResourceComposition resource)) {
                    project.ActiveTimeline = resource.Timeline;
                }
            }
        }
    }
}