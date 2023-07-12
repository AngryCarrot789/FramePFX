using System.Collections.Generic;
using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.AdvancedContextService;
using FramePFX.Core.Editor.ViewModels.Timelines;

namespace FramePFX.Core.Editor.Timelines.Context {
    public class TrackContextGenerator : IContextGenerator {
        public static TrackContextGenerator Instance { get; } = new TrackContextGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (context.TryGetContext(out TrackViewModel track)) {
                list.Add(new ActionContextEntry(null, "actions.resources.RenameItem", "Rename track"));
                list.Add(SeparatorEntry.Instance);
                list.Add(new ActionContextEntry(null, "actions.editor.NewVideoTrack", "Insert video track below"));
                list.Add(new ActionContextEntry(null, "actions.editor.NewAudioTrack", "Insert audio track below"));
                list.Add(new ActionContextEntry(null, "actions.resources.RenameItem"));
            }
            else if (context.TryGetContext(out TimelineViewModel _)) {
                list.Add(new ActionContextEntry(null, "actions.editor.NewVideoTrack", "Add Video track"));
                list.Add(new ActionContextEntry(null, "actions.editor.NewAudioTrack", "Add Audio Track"));
            }
        }
    }
}