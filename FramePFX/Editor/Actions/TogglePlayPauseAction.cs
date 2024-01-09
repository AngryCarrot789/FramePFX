using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels;

namespace FramePFX.Editor.Actions {
    public class TogglePlayPauseAction : ContextAction {
        public override async Task ExecuteAsync(ContextActionEventArgs e) {
            if (EditorActionUtils.GetVideoEditor(e.DataContext, out VideoEditorViewModel editor)) {
                await editor.Playback.TogglePlayAction();
            }
        }
    }
}