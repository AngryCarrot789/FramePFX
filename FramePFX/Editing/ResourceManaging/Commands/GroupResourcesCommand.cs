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
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.ResourceManaging.Commands;

public class GroupResourcesCommand : Command {
    public override Executability CanExecute(CommandEventArgs e) {
        return DataKeys.ResourceListUIKey.IsPresent(e.ContextData) && DataKeys.ResourceObjectKey.IsPresent(e.ContextData) ? Executability.Valid : Executability.Invalid;
    }

    protected override void Execute(CommandEventArgs e) {
        if (!DataKeys.ResourceObjectKey.IsPresent(e.ContextData))
            return;

        ResourceFolder dest;
        List<BaseResource> resources;
        if (DataKeys.ResourceListUIKey.TryGetContext(e.ContextData, out IResourceListElement? list)) {
            resources = list.Selection.SelectedItems.ToList();
            dest = (ResourceFolder?) list.CurrentFolderNode?.Resource ?? list.ManagerUI.ResourceManager!.RootContainer;
        }
        else {
            return;
        }

        // Safety post processing
        resources = resources.Where(x => x.Parent == dest).ToList();
        if (resources.Count < 1) {
            return;
        }

        int minIndex = resources.Min(x => dest.IndexOf(x));
        if (minIndex == -1) {
            throw new Exception("Fatal error, item was not in the target group");
        }

        ResourceFolder folder = new ResourceFolder("New Folder");
        dest.InsertItem(minIndex, folder);
        foreach (BaseResource res in resources) {
            res.Parent!.MoveItemTo(folder, res);
        }

        list.ManagerUI.Selection.Tree.Clear();
        list.Selection.SetSelection(folder);
    }
}