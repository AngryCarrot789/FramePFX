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

public class TreePath {
    /// <summary>
    /// Gets the tree associated with this path
    /// </summary>
    public readonly IResourceTreeElement Tree;
        
    /// <summary>
    /// Gets the node involved. Null means it involved the root of the tree instead of a specific node
    /// </summary>
    public readonly IResourceTreeNodeElement? Node;

    public TreePath(IResourceTreeElement tree, IResourceTreeNodeElement? node) {
        this.Tree = tree;
        this.Node = node;
    }
}