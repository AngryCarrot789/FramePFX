using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.ResourceManaging.Actions {
    public class GroupResourcesAction : AnAction {
        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(DataKeys.ResourceObjectKey, out BaseResource focusedResource) || focusedResource.Manager == null) {
                return Task.CompletedTask;
            }

            ResourceManager manager = focusedResource.Manager;
            ResourceFolder currFolder = manager.CurrentFolder;
            List<BaseResource> selection = currFolder.Items.Where(x => x.IsSelected).ToList();
            if (!focusedResource.IsSelected && currFolder.Items.Contains(focusedResource)) {
                selection.Add(focusedResource);
            }

            if (selection.Count > 0) {
                foreach (BaseResource resource in selection) {
                    resource.IsSelected = false;
                }

                ResourceFolder folder = new ResourceFolder("Grouped Folder");
                currFolder.AddItem(folder);
                foreach (BaseResource resource in selection) {
                    currFolder.MoveItemTo(folder, resource);
                }
            }

            return Task.CompletedTask;
        }
    }
}