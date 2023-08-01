using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ViewModels;

namespace FramePFX.Core.Editor.Actions {
    public class TogglePlayPauseAction : EditorAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!GetVideoEditor(e.DataContext, out VideoEditorViewModel editor)) {
                return false;
            }

            // await editor.Playback.TogglePlayAction();
            return true;
        }
    }
}