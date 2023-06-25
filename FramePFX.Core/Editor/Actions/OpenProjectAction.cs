using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline;

namespace FramePFX.Core.Editor.Actions {
    [ActionRegistration("actions.project.Open")]
    public class OpenProjectAction : AnAction {
        public static bool GetEditor(IDataContext context, out VideoEditorViewModel editor) {
            if (context.TryGetContext(out ClipViewModel clip) && clip.Track != null && (editor = clip.Track.Timeline.Project.Editor) != null) {
                return true;
            }
            else if (context.TryGetContext(out TimelineViewModel timeline) && (editor = timeline.Project.Editor) != null) {
                return true;
            }
            else if (context.TryGetContext(out ProjectViewModel project) && (editor = project.Editor) != null) {
                return true;
            }
            else if (context.TryGetContext(out editor)) {
                return true;
            }
            else {
                return false;
            }
        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!GetEditor(e.DataContext, out VideoEditorViewModel project)) {
                return false;
            }

            await project.OpenProjectAction();
            return true;
        }
    }
}