using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions {
    public class PlayAtLastFrameAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!EditorActionUtils.GetVideoEditor(e.DataContext, out VideoEditorViewModel editor) || editor.SelectedTimeline == null) {
                return false;
            }

            TimelineViewModel timeline = editor.SelectedTimeline;
            long frame = timeline.LastPlayHeadSeek;
            try {
                timeline.DoNotSetLastPlayHeadSeek = true;
                await editor.Playback.PlayFromFrame(frame);
            }
            finally {
                timeline.DoNotSetLastPlayHeadSeek = false;
            }

            return true;
        }
    }
}