using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels;

namespace FramePFX.Editor.Actions {
    public class OpenProjectAction : ExecutableAction {
        public OpenProjectAction() {
        }

        public override bool CanExecute(ActionEventArgs e) {
            return EditorActionUtils.GetVideoEditor(e.DataContext, out VideoEditorViewModel editor);
        }

        public override async Task<bool> ExecuteAsync(ActionEventArgs e) {
            if (!EditorActionUtils.GetVideoEditor(e.DataContext, out VideoEditorViewModel editor))
                return false;
            await editor.OpenProjectAction();
            return true;
        }
    }
}