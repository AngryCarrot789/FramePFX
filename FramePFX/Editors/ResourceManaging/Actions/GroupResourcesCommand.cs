using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editors.Contextual;

namespace FramePFX.Editors.ResourceManaging.Actions {
    public class GroupResourcesCommand : Command {
        public override Task ExecuteAsync(CommandEventArgs e) {
            if (!ResourceContextRegistry.GetSingleFolderSelectionContext(e.DataContext, out ResourceFolder currFolder, out BaseResource[] items)) {
                return Task.CompletedTask;
            }

            foreach (BaseResource resource in items) {
                resource.IsSelected = false;
            }

            ResourceFolder folder = new ResourceFolder("Grouped Folder");
            currFolder.AddItem(folder);
            foreach (BaseResource resource in items) {
                currFolder.MoveItemTo(folder, resource);
            }

            return Task.CompletedTask;
        }
    }
}