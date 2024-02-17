using FramePFX.CommandSystem;
using FramePFX.Editors.Contextual;

namespace FramePFX.Editors.ResourceManaging.Actions {
    public class GroupResourcesCommand : Command {
        public override void Execute(CommandEventArgs e) {
            if (!ResourceContextRegistry.GetSingleFolderSelectionContext(e.DataContext, out ResourceFolder currFolder, out BaseResource[] items)) {
                return;
            }

            foreach (BaseResource resource in items) {
                resource.IsSelected = false;
            }

            ResourceFolder folder = new ResourceFolder("Grouped Folder");
            currFolder.AddItem(folder);
            foreach (BaseResource resource in items) {
                currFolder.MoveItemTo(folder, resource);
            }
        }
    }
}