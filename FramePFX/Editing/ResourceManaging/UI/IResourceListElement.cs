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

using FramePFX.Interactivity;

namespace FramePFX.Editing.ResourceManaging.UI;

/// <summary>
/// An interface the resource list UI element
/// </summary>
public interface IResourceListElement {
    /// <summary>
    /// Gets the resource manager associated with this list item
    /// </summary>
    IResourceManagerElement ManagerUI { get; }

    /// <summary>
    /// Gets the folder TREE NODE being presented. This is a helper property to access the tree node from our <see cref="ManagerUI"/>
    /// </summary>
    IResourceTreeNodeElement? CurrentFolderTreeNode { get; }

    /// <summary>
    /// Gets the folder list element currently being presented. This is effectively the list equivalent of <see cref="CurrentFolderTreeNode"/>
    /// </summary>
    IResourceListItemElement? CurrentFolderItem { get; }

    /// <summary>
    /// Gets the resource list's selection manager. This may or may not be synced with the resource manager UI selection
    /// </summary>
    ISelectionManager<BaseResource> Selection { get; }
}