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

using System.Collections.ObjectModel;

namespace FramePFX.Utils.Collections.Observable;

public class ReadOnlyObservableList<T> : ReadOnlyCollection<T>, IObservableList<T>
{
    public event ObservableListMultipleItemsEventHandler<T>? ItemsAdded;
    public event ObservableListMultipleItemsEventHandler<T>? ItemsRemoved;
    public event ObservableListReplaceEventHandler<T>? ItemReplaced;
    public event ObservableListSingleItemEventHandler<T>? ItemMoved;

    public ReadOnlyObservableList(IObservableList<T> list) : base(list)
    {
        list.ItemsAdded += this.ListOnItemsAdded;
        list.ItemsRemoved += this.ListOnItemsRemoved;
        list.ItemReplaced += this.ListOnItemReplaced;
        list.ItemMoved += this.ListOnItemMoved;
    }

    private void ListOnItemsAdded(IObservableList<T> list, IList<T> items, int index) => this.ItemsAdded?.Invoke(this, items, index);

    private void ListOnItemsRemoved(IObservableList<T> list, IList<T> items, int index) => this.ItemsRemoved?.Invoke(this, items, index);

    private void ListOnItemReplaced(IObservableList<T> list, T olditem, T newitem, int index) => this.ItemReplaced?.Invoke(this, olditem, newitem, index);

    private void ListOnItemMoved(IObservableList<T> list, T item, int oldindex, int newindex) => this.ItemMoved?.Invoke(this, item, oldindex, newindex);
}