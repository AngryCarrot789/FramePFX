using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Core.Editor.ResourceManaging.Actions {
    public class RenameResourceAction: AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (e.DataContext.TryGetContext(out IRenameable item)) {
                await item.RenameAsync();
                return true;
            }
            else {
                return false;
            }
        }
    }
}