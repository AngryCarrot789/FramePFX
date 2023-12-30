using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Editor.ResourceManaging.Actions {
    public class GroupSelectedResourcesAction : ContextAction {
        public override async Task ExecuteAsync(ContextActionEventArgs e) {
            if (!ResourceActionUtils.GetSelectedResources(e.DataContext, out ResourceManagerViewModel manager, out List<BaseResourceViewModel> selection)) {
                return;
            }

            ResourceFolderViewModel current = manager.CurrentFolder;
            if (selection.Count > 0) {
                manager.SelectedItems.Clear();
                ResourceFolderViewModel newFolder = new ResourceFolderViewModel(new ResourceFolder("New Group"));
                if (await newFolder.RenameAsync()) {
                    current.AddItem(newFolder);
                    foreach (BaseResourceViewModel item in selection) {
                        if (item != current && item.Parent == current) {
                            current.Model.MoveItemTo(newFolder.Model, item.Model);
                        }
                    }
                }
            }
        }
    }
}