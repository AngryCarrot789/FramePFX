using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ViewModels.Timeline;

namespace FramePFX.Core.History.Actions {
    [ActionRegistration("actions.project.history.Redo")]
    public class RedoAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (UndoAction.TryGetHistoryManager(e.DataContext, out var manager, out var editor)) {
                if (manager.CanRedo) {
                    await manager.RedoAction();
                }
                else {
                    editor.View.PushNotificationMessage("Nothing to redo!");
                }

                return true;
            }
            else {
                return false;
            }
        }
    }
}