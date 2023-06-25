using System.Collections.Generic;
using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.AdvancedContextService;
using FramePFX.Core.Editor.ViewModels.Timeline;

namespace FramePFX.Core.Editor.Timeline.Context {
    public class TrackContextGenerator : IContextGenerator {
        public static TrackContextGenerator Instance { get; } = new TrackContextGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (context.TryGetContext(out TrackViewModel track)) {
                list.Add(new ActionContextEntry(track, "actions.resources.RenameItem"));
                list.Add(SeparatorEntry.Instance);
                list.Add(new ActionContextEntry(track.Timeline, "actions.editor.NewVideoTrack"));
                list.Add(new ActionContextEntry(track.Timeline, "actions.editor.NewAudioTrack"));
                list.Add(new ActionContextEntry(track, "actions.resources.RenameItem"));
            }
            else if (context.TryGetContext(out TimelineViewModel timeline)) {
                list.Add(new ActionContextEntry(timeline, "actions.editor.NewVideoTrack"));
                list.Add(new ActionContextEntry(timeline, "actions.editor.NewAudioTrack"));
            }
        }
    }
}