using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels;

namespace FramePFX.Editor.Actions {
    public class OpenProjectAction : EditorAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!GetVideoEditor(e.DataContext, out VideoEditorViewModel editor)) {
                return false;
            }

            await editor.OpenProjectAction();
            return true;
        }
    }
}