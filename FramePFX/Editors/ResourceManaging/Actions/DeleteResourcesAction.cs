using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.ResourceManaging.Actions {
    public class DeleteResourcesAction : AnAction {
        public override Task ExecuteAsync(AnActionEventArgs e) {
            IDataContext context = e.DataContext;
            if (!context.TryGetContext(DataKeys.ResourceObjectKey, out BaseResource resource)) {
                return Task.CompletedTask;
            }

            HashSet<BaseResource> resources = new HashSet<BaseResource>(resource.Manager.SelectedItems);
            if (!resource.IsSelected)
                resources.Add(resource);

            foreach (BaseResource item in resources) {
                // since it's a hash set, we might end up removing a folder containing some
                // selected items, so parent will be null since it deletes the hierarchy
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