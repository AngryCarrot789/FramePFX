using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Core.Editor.ResourceManaging.Actions {
    [ActionRegistration("actions.resources.RenameItem")]
    public class RenameResourceAction: AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            ResourceManagerViewModel manager;
            if (!e.DataContext.TryGetContext(out manager)) {
                if (!e.DataContext.TryGetContext(out ResourceItemViewModel resItem)) {
                    return false;
                }

                manager = resItem.Manager;
            }

            if (manager.SelectedItems.Count != 1) {
                return false;
            }

            await manager.RenameResourceAction(manager.SelectedItems[0]);
            return true;
        }
    }
}