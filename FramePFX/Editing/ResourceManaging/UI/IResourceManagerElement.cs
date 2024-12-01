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

namespace FramePFX.Editing.ResourceManaging.UI;

public interface IResourceManagerElement {
    ResourceManager? ResourceManager { get; }

    /// <summary>
    /// Gets the resource selection manager
    /// </summary>
    IResourceSelectionManager Selection { get; }

    /// <summary>
    /// Gets our resource list
    /// </summary>
    IResourceTreeElement Tree { get; }

    /// <summary>
    /// Gets our resource list
    /// </summary>
    IResourceListElement List { get; }

    /// <summary>
    /// Gets the node for the given resource
    /// </summary>
    /// <param name="resource">The model</param>
    /// <returns>The UI</returns>
    IResourceTreeNodeElement GetNode(BaseResource resource);
}