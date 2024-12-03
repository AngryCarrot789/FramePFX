using FramePFX.CommandSystem;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.Commands;

public class GroupResourcesCommand : Command {
    public override Executability CanExecute(CommandEventArgs e) {
        return DataKeys.ResourceListUIKey.GetExecutabilityForPresence(e.ContextData);
    }

    protected override void Execute(CommandEventArgs e) {
        ResourceFolder dest;
        List<BaseResource> resources;
        if (DataKeys.ResourceListUIKey.TryGetContext(e.ContextData, out IResourceListElement? list)) {
            resources = list.Selection.SelectedItems.ToList();
            dest = (ResourceFolder?) list.CurrentFolder?.Resource ?? list.ManagerUI.ResourceManager!.RootContainer;
        }
        else {
            return;
        }

        ResourceFolder folder = new ResourceFolder("New Folder");
        dest.AddItem(folder);
        foreach (BaseResource res in resources) {
            res.Parent!.MoveItemTo(folder, res);
        }
        
        // else if (DataKeys.ResourceTreeUIKey.TryGetContext(e.ContextData, out IResourceTreeElement? tree)) {
        // }
    }
}