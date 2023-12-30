using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.History.ViewModels;
using FramePFX.Notifications.Types;

namespace FramePFX.History.Actions {
    public class RedoAction : ContextAction {
        public override async Task ExecuteAsync(ContextActionEventArgs e) {
            HistoryManagerViewModel manager = HistoryManagerViewModel.Instance;
            if (manager.HasRedoActions) {
                await manager.RedoAction();
            }
            else if (manager.NotificationPanel != null) {
                manager.NotificationPanel.PushNotification(new MessageNotification("Cannot redo", "There is nothing to redo!"));
            }
        }
    }
}