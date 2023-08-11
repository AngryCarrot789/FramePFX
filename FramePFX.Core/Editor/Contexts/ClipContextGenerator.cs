using System.Collections.Generic;
using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.AdvancedContextService;
using FramePFX.Core.Editor.ViewModels.Timelines;

namespace FramePFX.Core.Editor.Contexts {
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
        }
    }
}