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

namespace FramePFX.Editing.ResourceManaging;

public interface IResourceSelectionManager
{
    /// <summary>
    /// Gets our tree's selection manager
    /// </summary>
    public ISelectionManager<BaseResource> Tree { get; }

    /// <summary>
    /// Gets our list's selection manager
    /// </summary>
    public ISelectionManager<BaseResource> List { get; }

    /// <summary>
    /// Gets or sets if the tree and list selection should be synchronized.
    /// Setting this to true will immediately sync, but setting to false
    /// will not change any current selections.
    /// Default value is true
    /// </summary>
    public bool SyncTreeWithList { get; set; }
}