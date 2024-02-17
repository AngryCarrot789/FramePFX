using FramePFX.CommandSystem;
using FramePFX.Editors.Contextual;

namespace FramePFX.Editors.ResourceManaging.Actions {
    public class DeleteResourcesCommand : Command {
        public override void Execute(CommandEventArgs e) {
            if (!ResourceContextRegistry.GetTreeContext(e.DataContext, out BaseResource[] items)) {
                return;
            }

            foreach (BaseResource item in items) {
                // Since the tree's selected items will be unordered (hash set), we might end up removing
                // a folder containing some selected items, so parent will be null since it deletes the hierarchy
                if (item.Parent == null) {
                    continue;
                }

                ResourceFolder.ClearHierarchy(item as ResourceFolder);
                item.Parent.RemoveItem(item);
            }
        }
    }
}