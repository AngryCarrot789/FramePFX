using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions {
    public class PlayAtLastFrameAction : ExecutableAction {
        public override async Task<bool> ExecuteAsync(ActionEventArgs e) {
            if (!EditorActionUtils.GetEditorWithTimeline(e.DataContext, out VideoEditorViewModel editor, out TimelineViewModel timeline)) {
                return false;
            }

            await editor.Playback.PlayFromFrame(timeline, timeline.LastPlayHeadSeek, false);
            return true;
        }
    }
}