// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

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