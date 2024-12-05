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
/// An interface for the resource tree UI component
/// </summary>
public interface IResourceTreeElement {
    /// <summary>
    /// Gets our tree's resource manager UI component
    /// </summary>
    IResourceManagerElement ManagerUI { get; }

    /// <summary>
    /// Gets the resource tree's selection manager. This may be synced with the resource manager UI selection
    /// </summary>
    ISelectionManager<BaseResource> Selection { get; }
}