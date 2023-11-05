using System.Collections.Generic;
using FramePFX.Actions.Contexts;
using FramePFX.AdvancedContextService;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Contexts {
    public class TrackListContextGenerator : IContextGenerator {
        public static TrackListContextGenerator Instance { get; } = new TrackListContextGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (context.TryGetContext(out TrackViewModel track)) {
                list.Add(new ActionContextEntry("actions.general.RenameItem", "Rename track"));
                list.Add(new ActionContextEntry("actions.timeline.track.ChangeTrackColour", "Change colour"));
                list.Add(SeparatorEntry.Instance);
                list.Add(new ActionContextEntry("actions.editor.NewVideoTrack", "Insert video track below"));
                list.Add(new ActionContextEntry("actions.editor.NewAudioTrack", "Insert audio track below"));
                list.Add(SeparatorEntry.Instance);
                list.Add(new CommandContextEntry("Delete track(s)", track.Timeline.RemoveSelectedTracksCommand));
            }
            else if (context.TryGetContext(out TimelineViewModel timeline)) {
                list.Add(new ActionContextEntry("actions.editor.NewVideoTrack", "Add Video track"));
                list.Add(new ActionContextEntry("actions.editor.NewAudioTrack", "Add Audio Track"));
            }
        }
    }
}