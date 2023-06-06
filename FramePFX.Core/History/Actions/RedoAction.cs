using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ViewModels.Timeline;

namespace FramePFX.Core.History.Actions {
    public class UndoAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (e.DataContext.TryGetContext(out TimelineViewModel timeline)) {
                await timeline.HistoryManager.UndoAction();
                return true;
            }
            else {
                return false;
            }
        }
    }
}