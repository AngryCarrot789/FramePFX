using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels;

namespace FramePFX.Editor.Actions {
    public class SaveProjectAction : ContextAction {
        public override async Task ExecuteAsync(ContextActionEventArgs e) {
            if (EditorActionUtils.GetProject(e.DataContext, out ProjectViewModel project)) {
                await project.SaveActionAsync();
            }
        }
    }

    public class SaveProjectAsAction : ContextAction {
        public override async Task ExecuteAsync(ContextActionEventArgs e) {
            if (EditorActionUtils.GetProject(e.DataContext, out ProjectViewModel project)) {
                await project.SaveAsActionAsync();
            }
        }
    }
}