using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Editor.ResourceManaging.Actions {
    public class GroupSelectedResourcesAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (e.DataContext.TryGetContext(out BaseResourceObjectViewModel item) && item.Manager != null) {
                await item.Parent.GroupSelectionIntoNewGroupAction();
                return true;
            }
            else {
                return false;
            }
        }
    }
}