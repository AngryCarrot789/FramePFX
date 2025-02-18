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

using PFXToolKitUI.Configurations;

namespace PFXToolKitUI.Avalonia.Configurations.Trees;

/// <summary>
/// An interface for shared properties between a <see cref="ConfigurationTreeView"/> and <see cref="ConfigurationTreeViewItem"/>
/// </summary>
public interface IConfigurationTreeOrNode {
    /// <summary>
    /// Gets the configuration tree view associated with this node, or returns the current instance
    /// </summary>
    ConfigurationTreeView? ResourceTree { get; }

    /// <summary>
    /// Gets the parent node, or null if we are a root node or a tree
    /// </summary>
    ConfigurationTreeViewItem? ParentNode { get; }

    /// <summary>
    /// Gets this node's entry, or returns the "root" configuration entry which contains all the root level entries
    /// </summary>
    ConfigurationEntry? Entry { get; }

    /// <summary>
    /// Gets the node at the specific UI index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    ConfigurationTreeViewItem GetNodeAt(int index);

    void InsertNode(ConfigurationEntry item, int index);

    void InsertNode(ConfigurationTreeViewItem control, ConfigurationEntry resource, int index);

    void RemoveNode(int index, bool canCache = true);
}