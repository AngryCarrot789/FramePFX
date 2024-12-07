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

using System.Diagnostics.CodeAnalysis;
using FramePFX.CommandSystem;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.ResourceManaging.Commands;

public class ToggleOnlineStateCommand : AsyncCommand {
    protected override Executability CanExecuteOverride(CommandEventArgs e) {
        return DataKeys.ResourceTreeUIKey.IsPresent(e.ContextData) || DataKeys.ResourceListUIKey.IsPresent(e.ContextData) ? Executability.Valid : Executability.Invalid;
    }

    protected override async Task ExecuteAsync(CommandEventArgs e) {
        if (!GetTargetItems(e.ContextData, out List<ResourceItem>? items)) {
            return;
        }

        List<ResourceItem> enable = new List<ResourceItem>();
        foreach (ResourceItem item in items) {
            if (item.IsOnline) {
                item.Disable(true);
            }
            else {
                enable.Add(item);
            }
        }

        if (enable.Count > 0)
            await IoC.ResourceLoaderService.TryLoadResources(enable.Cast<BaseResource>().ToArray());
    }

    public static bool GetTargetItems(IContextData context, [NotNullWhen(true)] out List<ResourceItem>? items, bool requireAtLeastOne = true) {
        if (DataKeys.ResourceTreeUIKey.TryGetContext(context, out IResourceTreeElement? tree)) {
            items = tree.Selection.SelectedItems.OfType<ResourceItem>().ToList();
        }
        else if (DataKeys.ResourceListUIKey.TryGetContext(context, out IResourceListElement? list)) {
            items = list.Selection.SelectedItems.OfType<ResourceItem>().ToList();
        }
        else {
            items = null;
            return false;
        }

        if (DataKeys.ResourceObjectKey.TryGetContext(context, out BaseResource? resource)) {
            if (resource is ResourceItem item && !items.Contains(item))
                items.Add(item);
        }
        
        return !requireAtLeastOne || items.Count > 0;
    }
}