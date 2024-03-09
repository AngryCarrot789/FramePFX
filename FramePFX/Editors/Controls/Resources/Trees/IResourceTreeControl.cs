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

using FramePFX.Editors.ResourceManaging;

namespace FramePFX.Editors.Controls.Resources.Trees
{
    /// <summary>
    /// An interface for shared properties between a <see cref="ResourceTreeView"/> and <see cref="ResourceTreeViewItem"/>
    /// </summary>
    public interface IResourceTreeControl
    {
        ResourceTreeView ResourceTree { get; }

        ResourceTreeViewItem ParentNode { get; }

        MovedResource MovedResource { get; set; }

        BaseResource Resource { get; }

        ResourceTreeViewItem GetNodeAt(int index);

        void InsertNode(BaseResource item, int index);

        void InsertNode(ResourceTreeViewItem control, BaseResource resource, int index);

        void RemoveNode(int index, bool canCache = true);
    }
}