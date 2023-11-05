using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.History.ViewModels;
using FramePFX.Notifications.Types;

namespace FramePFX.History.Actions {
    public class UndoAction : ExecutableAction {
        public override async Task<bool> ExecuteAsync(ActionEventArgs e) {
            HistoryManagerViewModel manager = HistoryManagerViewModel.Instance;
            if (manager.HasUndoActions) {
                await manager.UndoAction();
            }
            else if (manager.NotificationPanel != null) {
                manager.NotificationPanel.PushNotification(new MessageNotification("Cannot undo", "There is nothing to undo!"));
            }

            return true;
        }
    }
}