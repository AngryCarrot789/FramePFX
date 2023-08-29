using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels;

namespace FramePFX.Editor.Actions {
    public class SaveProjectAction : EditorAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (GetProject(e.DataContext, out ProjectViewModel project)) {
                await project.SaveActionAsync();
                return true;
            }

            return false;
        }
    }

    public class SaveProjectAsAction : EditorAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!GetProject(e.DataContext, out ProjectViewModel project)) {
                return false;
            }

            await project.SaveAsActionAsync();
            return true;
        }
    }
}