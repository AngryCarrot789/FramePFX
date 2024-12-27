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

/// <summary>
/// An interface for a resource tree node UI element
/// </summary>
public interface IResourceTreeNodeElement {
    /// <summary>
    /// Gets the resource model associated with this node. Returns null when <see cref="Parent"/> is null
    /// </summary>
    BaseResource? Resource { get; }

    /// <summary>
    /// Gets the parent node of this node. Returns null when we're a top level node
    /// </summary>
    IResourceTreeNodeElement? Parent { get; }

    /// <summary>
    /// Gets the tree that this node exists in. Returns null when we're not in a tree (maybe this node was removed but still referenced)
    /// </summary>
    IResourceTreeElement? Tree { get; }

    /// <summary>
    /// Gets or sets if this node is selected
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets the editing name state, which when true will show a text box to
    /// edit the name and when false just shows plain text
    /// </summary>
    bool EditNameState { get; set; }
}