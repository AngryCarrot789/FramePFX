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

namespace FramePFX.Editing.ResourceManaging.ResourceHelpers;

public delegate void EntryResourceChangedEventHandler(ResourceHelper sender, ResourceChangedEventArgs e);

public delegate void EntryResourceModifiedEventHandler(ResourceHelper sender, ResourceModifiedEventArgs e);

public delegate void EntryOnlineStateChangedEventHandler(ResourceHelper sender, IBaseResourcePathKey key);

public readonly struct ResourceModifiedEventArgs {
    /// <summary>
    /// The entry linked to the resource which was modified
    /// </summary>
    public IBaseResourcePathKey Key { get; }

    /// <summary>
    /// The item whose property changed
    /// </summary>
    public ResourceItem Item { get; }

    /// <summary>
    /// The property that changed
    /// </summary>
    public string Property { get; }

    public ResourceModifiedEventArgs(IBaseResourcePathKey key, ResourceItem item, string property) {
        this.Key = key;
        this.Item = item;
        this.Property = property;
    }
}

public readonly struct ResourceChangedEventArgs {
    /// <summary>
    /// The entry linked to the resource which was modified
    /// </summary>
    public IBaseResourcePathKey Key { get; }

    /// <summary>
    /// The previous item
    /// </summary>
    public ResourceItem OldItem { get; }

    /// <summary>
    /// The new item
    /// </summary>
    public ResourceItem NewItem { get; }

    public ResourceChangedEventArgs(IBaseResourcePathKey key, ResourceItem oldItem, ResourceItem newItem) {
        this.Key = key;
        this.OldItem = oldItem;
        this.NewItem = newItem;
    }
}