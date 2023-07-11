using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Core.Editor.ResourceManaging.Actions {
    public class GroupSelectedResourcesAction: AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (e.DataContext.TryGetContext(out BaseResourceObjectViewModel item) && item.Manager != null) {
                await item.Parent.GroupSelectionAction();
                return true;
            }
            else {
                return false;
            }
        }
    }
}