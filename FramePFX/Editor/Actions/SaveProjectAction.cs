using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels;

namespace FramePFX.Editor.Actions {
    public class SaveProjectAction : ExecutableAction {
        public override async Task<bool> ExecuteAsync(ActionEventArgs e) {
            if (!EditorActionUtils.GetProject(e.DataContext, out ProjectViewModel project))
                return false;
            await project.SaveActionAsync();
            return true;

        }
    }

    public class SaveProjectAsAction : ExecutableAction {
        public override async Task<bool> ExecuteAsync(ActionEventArgs e) {
            if (!EditorActionUtils.GetProject(e.DataContext, out ProjectViewModel project))
                return false;
            await project.SaveAsActionAsync();
            return true;
        }
    }
}