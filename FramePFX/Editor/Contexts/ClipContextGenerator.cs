using System.Collections.Generic;
using FramePFX.Actions.Contexts;
using FramePFX.AdvancedContextService;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Contexts {
    public class ClipContextGenerator : IContextGenerator {
        public static ClipContextGenerator Instance { get; } = new ClipContextGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (!context.TryGetContext(out ClipViewModel clip)) {
                return;
            }

            list.Add(new CommandContextEntry("Remove", clip.RemoveClipCommand));
            if (clip.AutomationData.ActiveSequence != null) {
                list.Add(new ActionContextEntry(null, "actions.automation.AddKeyFrame", "Add key frame"));
            }

            list.Add(SeparatorEntry.Instance);
            list.Add(new ActionContextEntry(clip, "actions.editor.timeline.CreateCompositionFromSelection", "Create composition from selection", "Creates a composition clip from the selected clips"));
        }
    }
}