using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions {
    [ActionRegistration("actions.editor.timeline.ToggleAutoScrollDuringPlayback")]
    public class ToggleAutoScrollDuringPlaybackAction : ExecutableAction {
        public override bool CanExecute(ActionEventArgs e) {
            return EditorActionUtils.GetTimeline(e.DataContext, out _);
        }

        public override Task<bool> ExecuteAsync(ActionEventArgs e) {
            if (!EditorActionUtils.GetTimeline(e.DataContext, out TimelineViewModel timeline)) {
                return Task.FromResult(false);
            }

            timeline.AutoScrollDuringPlayback = !timeline.AutoScrollDuringPlayback;
            return Task.FromResult(true);
        }
    }
}