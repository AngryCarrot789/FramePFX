using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editors.Contextual;

namespace FramePFX.Editors.ResourceManaging.Actions {
    public class DeleteResourcesCommand : Command {
        public override Task ExecuteAsync(CommandEventArgs e) {
            if (!ResourceContextRegistry.GetTreeContext(e.DataContext, out BaseResource[] items)) {
                return Task.CompletedTask;
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

            return Task.CompletedTask;
        }
    }
}