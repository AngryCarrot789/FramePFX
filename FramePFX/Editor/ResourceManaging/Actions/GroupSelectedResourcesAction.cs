using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Editor.ResourceManaging.Actions {
    public class GroupSelectedResourcesAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            ResourceManagerViewModel manager;
            ResourceFolderViewModel folder;
            if (e.DataContext.TryGetContext(out manager)) {
                folder = manager.CurrentFolder;
            }
            else if (e.DataContext.TryGetContext(out BaseResourceViewModel item) && (manager = item.Manager) != null) {
                folder = manager.CurrentFolder;
            }
            else {
                return false;
            }

            if (manager.SelectedItems.Count > 0) {
                // comparing x.Parent is faster than target.Items.Contains(x)
                List<BaseResourceViewModel> list = manager.SelectedItems.Where(x => x != folder && x.Parent == folder).ToList();
                manager.SelectedItems.Clear();
                await GroupSelectionIntoNewGroupAction(manager, folder, list);
            }

            return true;
        }

        public static async Task GroupSelectionIntoNewGroupAction(ResourceManagerViewModel manager, ResourceFolderViewModel folder, List<BaseResourceViewModel> items) {
            ResourceFolderViewModel newFolder = new ResourceFolderViewModel(new ResourceFolder("New Group"));
            if (!await newFolder.RenameAsync()) {
                return;
            }

            // assert group == item.Parent
            foreach (BaseResourceViewModel item in items) {
                item.Parent.RemoveItem(item, true, false);
            }

            folder.AddItem(newFolder);
            foreach (BaseResourceViewModel item in items) {
                newFolder.AddItem(item);
            }
        }
    }
}