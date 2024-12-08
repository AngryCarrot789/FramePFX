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

namespace FramePFX.Utils.Collections.Observable;

public delegate void ObservableItemProcessorItemEventHandler<in T>(object sender, int index, T item);

public delegate void ObservableItemProcessorItemMovedEventHandler<in T>(object sender, int oldIndex, int newIndex, T item);

/// <summary>
/// A helper class for creating observable item processors
/// </summary>
public static class ObservableItemProcessor
{
    /// <summary>
    /// Creates a simple item processor that invokes two callbacks when an item is added to or removed from the collection 
    /// </summary>
    /// <param name="list"></param>
    /// <param name="onItemAdded"></param>
    /// <param name="onItemRemoved"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ObservableItemProcessorSimple<T> MakeSimple<T>(IObservableList<T> list, Action<T>? onItemAdded, Action<T>? onItemRemoved)
    {
        return new ObservableItemProcessorSimple<T>(list, onItemAdded, onItemRemoved);
    }

    /// <summary>
    /// Creates an item processor that provides the index of added and removed items
    /// </summary>
    /// <param name="list"></param>
    /// <param name="onItemAdded"></param>
    /// <param name="onItemRemoved"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ObservableItemProcessorIndexing<T> MakeIndexable<T>(IObservableList<T> list, ObservableItemProcessorItemEventHandler<T>? onItemAdded, ObservableItemProcessorItemEventHandler<T>? onItemRemoved, ObservableItemProcessorItemMovedEventHandler<T>? onItemMoved, bool useOptimisedRemovalProcessing = true)
    {
        return new ObservableItemProcessorIndexing<T>(list, onItemAdded, onItemRemoved, onItemMoved, useOptimisedRemovalProcessing);
    }
}

public sealed class ObservableItemProcessorSimple<T> : IDisposable
{
    private readonly IObservableList<T> list;

    public event Action<T>? OnItemAdded;
    public event Action<T>? OnItemRemoved;

    public ObservableItemProcessorSimple(IObservableList<T> list, Action<T>? itemAdded, Action<T>? itemRemoved)
    {
        this.list = list ?? throw new ArgumentNullException(nameof(list));
        list.ItemsAdded += this.OnItemsAdded;
        list.ItemsRemoved += this.OnItemsRemoved;
        list.ItemReplaced += this.OnItemReplaced;

        this.OnItemAdded = itemAdded;
        this.OnItemRemoved = itemRemoved;
    }

    private void OnItemsAdded(IObservableList<T> observableList, IList<T> items, int index)
    {
        Action<T>? handler = this.OnItemAdded;
        if (handler == null)
            return;

        foreach (T item in items)
            handler(item);
    }

    private void OnItemsRemoved(IObservableList<T> observableList, IList<T> items, int index)
    {
        Action<T>? handler = this.OnItemRemoved;
        if (handler == null)
            return;

        foreach (T item in items)
            handler(item);
    }

    private void OnItemReplaced(IObservableList<T> observableList, T olditem, T newitem, int index)
    {
        this.OnItemRemoved?.Invoke(olditem);
        this.OnItemAdded?.Invoke(newitem);
    }

    public void Dispose()
    {
        this.list.ItemsAdded -= this.OnItemsAdded;
        this.list.ItemsRemoved -= this.OnItemsRemoved;
        this.list.ItemReplaced -= this.OnItemReplaced;
    }
}

public sealed class ObservableItemProcessorIndexing<T> : IDisposable
{
    private readonly IObservableList<T> list;
    private readonly bool useOptimisedRemovalProcessing;

    public event ObservableItemProcessorItemEventHandler<T>? OnItemAdded;
    public event ObservableItemProcessorItemEventHandler<T>? OnItemRemoved;
    public event ObservableItemProcessorItemMovedEventHandler<T>? OnItemMoved;

    public ObservableItemProcessorIndexing(IObservableList<T> list, ObservableItemProcessorItemEventHandler<T>? itemAdded, ObservableItemProcessorItemEventHandler<T>? itemRemoved, ObservableItemProcessorItemMovedEventHandler<T>? itemMoved, bool useOptimisedRemovalProcessing)
    {
        this.list = list ?? throw new ArgumentNullException(nameof(list));
        this.useOptimisedRemovalProcessing = useOptimisedRemovalProcessing;
        list.ItemsAdded += this.ItemsAdded;
        list.ItemsRemoved += this.ItemsRemoved;
        list.ItemReplaced += this.ItemReplaced;
        list.ItemMoved += this.ItemMoved;

        this.OnItemAdded = itemAdded;
        this.OnItemRemoved = itemRemoved;
        this.OnItemMoved = itemMoved;
    }

    private void ItemsAdded(IObservableList<T> observableList, IList<T> items, int index)
    {
        ObservableItemProcessorItemEventHandler<T>? handler = this.OnItemAdded;
        if (handler == null)
            return;

        int i = index - 1;
        foreach (T item in items)
            handler(this, ++i, item);
    }

    private void ItemsRemoved(IObservableList<T> observableList, IList<T> items, int index)
    {
        ObservableItemProcessorItemEventHandler<T>? handler = this.OnItemRemoved;
        if (handler == null)
            return;

        if (this.useOptimisedRemovalProcessing)
        {
            // Remove back to front, as it's usually the most performant for array-based
            // list implementations that may be modified by the handler,
            // since they will do overall less copying
            for (int j = items.Count, i = index + j - 1; i >= index; i--)
            {
                // i = index of last item in unaware listeners' list, points to junk in real list
                // j = index in items parameter list
                handler(this, i, items[--j]);
            }
        }
        else
        {
            foreach (T item in items)
                handler(this, index, item);
        }
    }

    private void ItemReplaced(IObservableList<T> observableList, T olditem, T newitem, int index)
    {
        this.OnItemRemoved?.Invoke(this, index, olditem);
        this.OnItemAdded?.Invoke(this, index, newitem);
    }

    private void ItemMoved(IObservableList<T> observableList, T item, int oldIndex, int newIndex)
    {
        this.OnItemMoved?.Invoke(this, oldIndex, newIndex, item);
    }

    public void Dispose()
    {
        this.list.ItemsAdded -= this.ItemsAdded;
        this.list.ItemsRemoved -= this.ItemsRemoved;
        this.list.ItemReplaced -= this.ItemReplaced;
        this.list.ItemMoved -= this.ItemMoved;
    }
}