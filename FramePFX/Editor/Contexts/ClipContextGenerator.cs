using System.Collections.Generic;
using FramePFX.Actions.Contexts;
using FramePFX.AdvancedContextService;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;

namespace FramePFX.Editor.Contexts {
    public class ClipContextGenerator : IContextGenerator {
        public static ClipContextGenerator Instance { get; } = new ClipContextGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (!context.TryGetContext(out ClipViewModel clip)) {
                return;
            }

            if (clip is CompositionVideoClipViewModel compositionClip && compositionClip.TryGetResource(out _)) {
                list.Add(new ActionContextEntry(clip, "actions.timeline.OpenCompositionObjectsTimeline", "Open timeline"));
            }

            list.Add(new CommandContextEntry("Remove", clip.RemoveClipCommand));
            list.Add(SeparatorEntry.Instance);
            list.Add(new ActionContextEntry(clip, "actions.automation.AddKeyFrame", "Add key frame", "Adds a key frame to the active sequence"));
            list.Add(new ActionContextEntry(clip, "actions.editor.timeline.CreateCompositionFromSelection", "Create composition from selection", "Creates a composition clip from the selected clips"));
            list.Add(SeparatorEntry.Instance);
            list.Add(new ActionContextEntry(clip, "actions.general.RenameItem", "Rename"));
        }
    }
}