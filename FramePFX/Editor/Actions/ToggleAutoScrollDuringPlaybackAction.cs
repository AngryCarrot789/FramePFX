using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions {
    [ActionRegistration("actions.editor.timeline.ToggleAutoScrollDuringPlayback")]
    public class ToggleAutoScrollDuringPlaybackAction : ContextAction {
        public override bool CanExecute(ContextActionEventArgs e) {
            return EditorActionUtils.GetTimeline(e.DataContext, out _);
        }

        public override Task ExecuteAsync(ContextActionEventArgs e) {
            if (!EditorActionUtils.GetTimeline(e.DataContext, out TimelineViewModel timeline)) {
                return Task.FromResult(false);
            }

            timeline.AutoScrollDuringPlayback = !timeline.AutoScrollDuringPlayback;
            return Task.FromResult(true);
        }
    }
}