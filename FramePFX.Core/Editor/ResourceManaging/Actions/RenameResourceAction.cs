using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Core.Editor.ResourceManaging.Actions {
    [ActionRegistration("actions.resources.RenameItem")]
    public class RenameResourceAction: AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (e.DataContext.TryGetContext(out ResourceItemViewModel item)) {
                await item.RenameSelfAction();
            }
            else if (e.DataContext.TryGetContext(out ResourceGroupViewModel group)) {
                await group.RenameSelfAction();
            }
            else {
                return false;
            }
            return true;
        }
    }
}