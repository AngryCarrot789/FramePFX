using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline;

namespace FramePFX.Core.Editor.Actions {
    [ActionRegistration("actions.project.Save")]
    public class SaveProjectAction : AnAction {
        public static bool GetProject(IDataContext context, out ProjectViewModel project) {
            if (context.TryGetContext(out ClipViewModel clip) && clip.Layer != null && (project = clip.Layer.Timeline.Project) != null) {
                return true;
            }
            else if (context.TryGetContext(out TimelineViewModel timeline) && (project = timeline.Project) != null) {
                return true;
            }
            else if (context.TryGetContext(out project)) {
                return true;
            }
            else if (context.TryGetContext(out project)) {
                return true;
            }
            else {
                return false;
            }
        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!GetProject(e.DataContext, out ProjectViewModel project)) {
                return false;
            }

            await project.SaveActionAsync();
            return true;
        }
    }

    [ActionRegistration("actions.project.SaveAs")]
    public class SaveProjectAsAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!SaveProjectAction.GetProject(e.DataContext, out ProjectViewModel project)) {
                return false;
            }

            await project.SaveAsActionAsync();
            return true;
        }
    }
}