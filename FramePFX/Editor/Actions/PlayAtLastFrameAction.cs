using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels;

namespace FramePFX.Editor.Actions {
    public class PlayAtLastFrameAction : EditorAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!GetVideoEditor(e.DataContext, out VideoEditorViewModel editor) || editor.ActiveTimeline == null) {
                return false;
            }

            long frame = editor.ActiveTimeline.LastSeekPlayHead;
            await editor.Playback.PlayFromFrame(frame);
            editor.ActiveTimeline.LastSeekPlayHead = frame;
            return true;
        }
    }
}