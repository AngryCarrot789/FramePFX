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

public interface IResourceListElement
{
    IResourceManagerElement ManagerUI { get; }

    /// <summary>
    /// Gets the folder TREE NODE being presented. This is a helper property to access the tree node from our <see cref="ManagerUI"/>
    /// </summary>
    IResourceTreeNodeElement? CurrentFolderNode { get; }

    /// <summary>
    /// Gets the folder item being presented
    /// </summary>
    IResourceListItemElement? CurrentFolderItem { get; }

    /// <summary>
    /// Gets the resource list's selection manager. This may be synced with the resource manager UI selection
    /// </summary>
    ISelectionManager<BaseResource> Selection { get; }
}