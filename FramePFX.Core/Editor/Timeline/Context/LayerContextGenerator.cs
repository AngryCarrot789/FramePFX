using System.Collections.Generic;
using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.AdvancedContextService;
using FramePFX.Core.Editor.ViewModels.Timeline;

namespace FramePFX.Core.Editor.Timeline.Context {
    public class LayerContextGenerator : IContextGenerator {
        public static LayerContextGenerator Instance { get; } = new LayerContextGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (context.TryGetContext(out LayerViewModel layer)) {
                list.Add(new ActionContextEntry(layer, "actions.resources.RenameItem"));
                list.Add(SeparatorEntry.Instance);
                list.Add(new ActionContextEntry(layer.Timeline, "actions.editor.NewVideoLayer"));
                list.Add(new ActionContextEntry(layer.Timeline, "actions.editor.NewAudioLayer"));
                list.Add(new ActionContextEntry(layer, "actions.resources.RenameItem"));
            }
            else if (context.TryGetContext(out TimelineViewModel timeline)) {
                list.Add(new ActionContextEntry(timeline, "actions.editor.NewVideoLayer"));
                list.Add(new ActionContextEntry(timeline, "actions.editor.NewAudioLayer"));
            }
        }
    }
}