// 
// Copyright (c) 2024-2024 REghZy
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

using System.Collections.Specialized;
using System.ComponentModel;

namespace FramePFX.Utils.Collections.Observable;

public delegate void ObservableListMultipleItemsEventHandler<T>(IObservableList<T> list, IList<T> items, int index);

public delegate void ObservableListSingleItemEventHandler<T>(IObservableList<T> list, T item, int oldIndex, int newIndex);

public delegate void ObservableListReplaceEventHandler<T>(IObservableList<T> list, T oldItem, T newItem, int index);

/// <summary>
/// A list implementation that invokes a series of events when the collection changes
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IObservableList<T> : IList<T> {
    /// <summary>
    /// An event fired when one or more items are inserted into the list.
    /// <para>
    /// The event args are the list of items inserted, and the index of the insertion
    /// </para>
    /// </summary>
    event ObservableListMultipleItemsEventHandler<T> ItemsAdded;

    /// <summary>
    /// An event fired when one or more items are removed from this list, at the given index. This is also fired
    /// when the list is cleared, where the index will equal 0 and the args contains all the items that were cleared
    /// <para>
    /// The event args are the list of items that were removed (which remain in the same order as
    /// they were in the list) and the original index of the first item in the removed items list
    /// </para>
    /// </summary>
    event ObservableListMultipleItemsEventHandler<T> ItemsRemoved;

    /// <summary>
    /// An event fired when an item is replaced. Only a single item is supported
    /// </summary>
    event ObservableListReplaceEventHandler<T> ItemReplaced;

    /// <summary>
    /// An event fired when an item is moved from one index to another
    /// </summary>
    event ObservableListSingleItemEventHandler<T> ItemMoved;
}