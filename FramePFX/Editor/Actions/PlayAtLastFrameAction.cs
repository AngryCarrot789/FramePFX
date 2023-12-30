using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions {
    public class PlayAtLastFrameAction : ContextAction {
        public override async Task ExecuteAsync(ContextActionEventArgs e) {
            if (EditorActionUtils.GetEditorWithTimeline(e.DataContext, out VideoEditorViewModel editor, out TimelineViewModel timeline)) {
                await editor.Playback.PlayFromFrame(timeline, timeline.LastPlayHeadSeek, false);
            }
        }
    }
}