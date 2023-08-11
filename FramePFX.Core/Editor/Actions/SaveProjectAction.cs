using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ViewModels;

namespace FramePFX.Core.Editor.Actions {
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