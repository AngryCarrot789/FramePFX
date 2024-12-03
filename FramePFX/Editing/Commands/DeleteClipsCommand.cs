using FramePFX.CommandSystem;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.UI;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.Commands;

public class DeleteClipsCommand : Command {
    protected override void Execute(CommandEventArgs e) {
        if (DataKeys.TimelineUIKey.TryGetContext(e.ContextData, out ITimelineElement? timeline)) {
            List<IClipElement> list = timeline.ClipSelection.SelectedItems.ToList();
            
            // Must clear the selection sine removing clips doesn't automatically de-selet them at the moment
            timeline.ClipSelection.Clear();
            
            foreach (IClipElement clip in list) {
                Clip model = clip.Clip;
                model.Track?.RemoveClip(model);
                model.Destroy();
            }
        }
    }
}