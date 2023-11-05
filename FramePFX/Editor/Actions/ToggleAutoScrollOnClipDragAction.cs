using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions {
    [ActionRegistration("actions.editor.timeline.ToggleAutoScrollOnClipDrag")]
    public class ToggleAutoScrollOnClipDragAction : ExecutableAction {
        public override Task<bool> ExecuteAsync(ActionEventArgs e) {
            if (!EditorActionUtils.GetTimeline(e.DataContext, out TimelineViewModel timeline)) {
                return Task.FromResult(false);
            }

            timeline.AutoScrollOnClipDrag = !timeline.AutoScrollOnClipDrag;
            return Task.FromResult(true);
        }
    }
}