using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels;

namespace FramePFX.Editor.Actions {
    public class TogglePlayPauseAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!EditorActionUtils.GetVideoEditor(e.DataContext, out VideoEditorViewModel editor) || editor.ActiveTimeline == null) {
                return false;
            }

            await editor.Playback.TogglePlayAction();
            return true;
        }
    }
}