using System.Collections.Generic;
using FramePFX.Actions.Contexts;
using FramePFX.AdvancedContextService;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Contexts {
    public class TrackContextGenerator : IContextGenerator {
        public static TrackContextGenerator Instance { get; } = new TrackContextGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (context.TryGetContext(out TrackViewModel track)) {
                list.Add(new ActionContextEntry(track, "actions.general.RenameItem", "Rename track"));
                list.Add(SeparatorEntry.Instance);
                list.Add(new ActionContextEntry(track, "actions.editor.NewVideoTrack", "Insert video track below"));
                list.Add(new ActionContextEntry(track, "actions.editor.NewAudioTrack", "Insert audio track below"));
                list.Add(SeparatorEntry.Instance);
                list.Add(new CommandContextEntry("Delete track", track.Timeline.RemoveSelectedTracksCommand));
            }
            else if (context.TryGetContext(out TimelineViewModel timeline)) {
                list.Add(new ActionContextEntry(timeline, "actions.editor.NewVideoTrack", "Add Video track"));
                list.Add(new ActionContextEntry(timeline, "actions.editor.NewAudioTrack", "Add Audio Track"));
            }
        }
    }
}