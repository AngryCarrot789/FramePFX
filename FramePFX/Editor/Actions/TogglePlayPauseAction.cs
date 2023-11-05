using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions {
    public class TogglePlayPauseAction : ExecutableAction {
        public override async Task<bool> ExecuteAsync(ActionEventArgs e) {
            if (!EditorActionUtils.GetVideoEditor(e.DataContext, out VideoEditorViewModel editor))
                return false;
            await editor.Playback.TogglePlayAction();
            return true;
        }
    }
}