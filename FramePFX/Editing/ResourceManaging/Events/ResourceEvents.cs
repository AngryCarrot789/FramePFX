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

namespace FramePFX.Editing.ResourceManaging.Events;

public delegate void ResourceEventHandler(BaseResource resource);

public delegate void ResourceItemEventHandler(ResourceItem resource);

public delegate void ResourceAddedEventHandler(ResourceFolder parent, BaseResource item, int index);

public delegate void ResourceRemovedEventHandler(ResourceFolder parent, BaseResource item, int index);

public delegate void ResourceMovedEventHandler(ResourceFolder sender, ResourceMovedEventArgs e);

/// <summary>
/// Event args for when a resource is moved from one folder to another
/// </summary>
public class ResourceMovedEventArgs : EventArgs {
    /// <summary>
    /// The source/original folder
    /// </summary>
    public ResourceFolder OldFolder { get; }

    /// <summary>
    /// The target/destination folder
    /// </summary>
    public ResourceFolder NewFolder { get; }

    /// <summary>
    /// The item that was moved
    /// </summary>
    public BaseResource Item { get; }

    /// <summary>
    /// The old index that <see cref="Item"/> was located at when it existed in <see cref="OldFolder"/>
    /// </summary>
    public int OldIndex { get; }

    /// <summary>
    /// The index of <see cref="Item"/> in <see cref="NewFolder"/>
    /// </summary>
    public int NewIndex { get; }

    /// <summary>
    /// Returns true when the resource was moved within the same folder, meaning it just moved positions
    /// </summary>
    public bool IsSameFolder => this.OldFolder == this.NewFolder;

    public ResourceMovedEventArgs(ResourceFolder oldFolder, ResourceFolder newFolder, BaseResource item, int oldIndex, int newIndex) {
        this.OldFolder = oldFolder;
        this.NewFolder = newFolder;
        this.Item = item;
        this.OldIndex = oldIndex;
        this.NewIndex = newIndex;
    }
}