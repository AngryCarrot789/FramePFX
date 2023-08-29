using System.Threading.Tasks;
using FramePFX.Actions;

namespace FramePFX.History.Actions {
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