using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Editor.ResourceManaging.Actions {
    public class GroupSelectedResourcesAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            ResourceManagerViewModel manager;
            ResourceGroupViewModel group;
            if (e.DataContext.TryGetContext(out manager)) {
                group = manager.CurrentGroup;
            }
            else if (e.DataContext.TryGetContext(out BaseResourceObjectViewModel item) && (manager = item.Manager) != null) {
                group = manager.CurrentGroup;
            }
            else {
                return false;
            }

            if (manager.SelectedItems.Count > 0) {
                // comparing x.Parent is faster than target.Items.Contains(x)
                List<BaseResourceObjectViewModel> list = manager.SelectedItems.Where(x => x != group && x.Parent == group).ToList();
                manager.SelectedItems.Clear();
                await GroupSelectionIntoNewGroupAction(manager, group, list);
            }

            return true;
        }

        public static async Task GroupSelectionIntoNewGroupAction(ResourceManagerViewModel manager, ResourceGroupViewModel group, List<BaseResourceObjectViewModel> items) {
            ResourceGroupViewModel newGroup = new ResourceGroupViewModel(new ResourceGroup("New Group"));
            if (!await newGroup.RenameAsync()) {
                return;
            }

            // assert group == item.Parent
            foreach (BaseResourceObjectViewModel item in items) {
                item.Parent.RemoveItem(item, true, false);
            }

            group.AddItem(newGroup);
            foreach (BaseResourceObjectViewModel item in items) {
                newGroup.AddItem(item);
            }
        }
    }
}