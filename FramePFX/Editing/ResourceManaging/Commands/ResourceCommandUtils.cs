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

public static class ResourceCommandUtils {
    /// <summary>
    /// Gets either the non-selected contextual resource, or the only selected item in either the list or tree
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="resource"></param>
    /// <returns></returns>
    public static bool GetSingleItem(IContextData ctx, [NotNullWhen(true)] out BaseResource? resource) {
        int count;
        bool isSelected;
        if (DataKeys.ResourceListUIKey.TryGetContext(ctx, out IResourceListElement? listElement)) {
            if ((count = listElement.Selection.Count) == 0) {
                return DataKeys.ResourceObjectKey.TryGetContext(ctx, out resource);
            }

            if (!DataKeys.ResourceObjectKey.TryGetContext(ctx, out resource)) {
                return false;
            }

            isSelected = listElement.Selection.IsSelected(resource);
        }
        else if (DataKeys.ResourceTreeUIKey.TryGetContext(ctx, out IResourceTreeElement? treeElement)) {
            if ((count = treeElement.Selection.Count) == 0) {
                return DataKeys.ResourceObjectKey.TryGetContext(ctx, out resource);
            }

            if (!DataKeys.ResourceObjectKey.TryGetContext(ctx, out resource)) {
                return false;
            }

            isSelected = treeElement.Selection.IsSelected(resource);
        }
        else {
            return DataKeys.ResourceObjectKey.TryGetContext(ctx, out resource);
        }

        return count == 1 ? isSelected : !isSelected;
    }

    public static Executability GetExecutabilityForSingleItem(IContextData ctx) {
        int count;
        bool isSelected;
        if (DataKeys.ResourceListUIKey.TryGetContext(ctx, out IResourceListElement? listElement)) {
            if ((count = listElement.Selection.Count) == 0) {
                return DataKeys.ResourceObjectKey.GetExecutabilityForPresence(ctx);
            }

            if (!DataKeys.ResourceObjectKey.TryGetContext(ctx, out BaseResource? resource)) {
                return Executability.Invalid;
            }

            isSelected = listElement.Selection.IsSelected(resource);
        }
        else if (DataKeys.ResourceTreeUIKey.TryGetContext(ctx, out IResourceTreeElement? treeElement)) {
            if ((count = treeElement.Selection.Count) == 0) {
                return DataKeys.ResourceObjectKey.GetExecutabilityForPresence(ctx);
            }

            if (!DataKeys.ResourceObjectKey.TryGetContext(ctx, out BaseResource? resource)) {
                return Executability.Invalid;
            }

            isSelected = treeElement.Selection.IsSelected(resource);
        }
        else {
            return DataKeys.ResourceObjectKey.GetExecutabilityForPresence(ctx);
        }

        return (count == 1 ? isSelected : !isSelected) ? Executability.Valid : Executability.ValidButCannotExecute;
    }
}