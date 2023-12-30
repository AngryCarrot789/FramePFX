using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels;

namespace FramePFX.Editor.Actions {
    public class OpenProjectAction : ContextAction {
        public OpenProjectAction() {
        }

        public override bool CanExecute(ContextActionEventArgs e) {
            return EditorActionUtils.GetVideoEditor(e.DataContext, out VideoEditorViewModel editor);
        }

        public override async Task ExecuteAsync(ContextActionEventArgs e) {
            if (EditorActionUtils.GetVideoEditor(e.DataContext, out VideoEditorViewModel editor)) {
                await editor.OpenProjectAction();
            }
        }
    }
}