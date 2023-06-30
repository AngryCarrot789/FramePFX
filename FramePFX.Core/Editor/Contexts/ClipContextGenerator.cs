using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.AdvancedContextService;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Automation.ViewModels;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Editor.ViewModels;
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