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
using System.Diagnostics;

namespace FramePFX.Utils.Collections.ObservableEx;

public class ObservableListEx<T> : Collection<T>, IObservableListEx<T>
{
    private SimpleMonitor? _monitor;
    private int blockReentrancyCount;

    public event ObservableListExChangedEventHandler<T>? CollectionChanged;

    public ObservableListEx() {
    }

    public ObservableListEx(IEnumerable<T> collection) : base(new List<T>(collection ?? throw new ArgumentNullException(nameof(collection)))) {
    }

    public ObservableListEx(List<T> list) : base(new List<T>(list ?? throw new ArgumentNullException(nameof(list)))) {
    }

    public void Move(int oldIndex, int newIndex) => this.MoveItem(oldIndex, newIndex);

    protected override void ClearItems()
    {
        this.CheckReentrancy();
        ObservableListChangedEventArgs<T> e = new ObservableListChangedEventArgs<T>(this.Items);
        base.ClearItems();
        this.OnCollectionChanged(e);
    }

    protected override void RemoveItem(int index)
    {
        this.CheckReentrancy();
        T removedItem = this[index];

        base.RemoveItem(index);
        this.OnCollectionChanged(new ObservableListChangedEventArgs<T>(ObservableListCollectionChangedAction.Remove, removedItem, default, index, -1));
    }

    protected override void InsertItem(int index, T item)
    {
        this.CheckReentrancy();
        base.InsertItem(index, item);
        this.OnCollectionChanged(new ObservableListChangedEventArgs<T>(ObservableListCollectionChangedAction.Add, default, item, -1, index));
    }

    protected override void SetItem(int index, T newItem)
    {
        this.CheckReentrancy();
        T oldItem = this[index];
        base.SetItem(index, newItem);
        this.OnCollectionChanged(new ObservableListChangedEventArgs<T>(ObservableListCollectionChangedAction.Replace, oldItem, newItem, index, index));
    }

    protected virtual void MoveItem(int oldIndex, int newIndex)
    {
        this.CheckReentrancy();

        T removedItem = this[oldIndex];

        base.RemoveItem(oldIndex);
        base.InsertItem(newIndex, removedItem);
        this.OnCollectionChanged(new ObservableListChangedEventArgs<T>(ObservableListCollectionChangedAction.Move, removedItem, removedItem, oldIndex, newIndex));
    }

    protected virtual void OnCollectionChanged(ObservableListChangedEventArgs<T> e)
    {
        ObservableListExChangedEventHandler<T>? handler = this.CollectionChanged;
        if (handler != null)
        {
            // Not calling BlockReentrancy() here to avoid the SimpleMonitor allocation.
            this.blockReentrancyCount++;
            try
            {
                handler(this, e);
            }
            finally
            {
                this.blockReentrancyCount--;
            }
        }
    }

    protected IDisposable BlockReentrancy()
    {
        this.blockReentrancyCount++;
        return this.EnsureMonitorInitialized();
    }

    protected void CheckReentrancy()
    {
        if (this.blockReentrancyCount > 0)
        {
            // we can allow changes if there's only one listener - the problem
            // only arises if reentrant changes make the original event args
            // invalid for later listeners.  This keeps existing code working
            // (e.g. Selector.SelectedItems).
            if (this.CollectionChanged?.GetInvocationList().Length > 1)
                throw new InvalidOperationException("Reentrancy Not Allowed");
        }
    }

    private SimpleMonitor EnsureMonitorInitialized() => this._monitor ??= new SimpleMonitor(this);

    private sealed class SimpleMonitor : IDisposable
    {
        internal int _busyCount; // Only used during (de)serialization to maintain compatibility with desktop. Do not rename (binary serialization)

        [NonSerialized] internal ObservableListEx<T> _collection;

        public SimpleMonitor(ObservableListEx<T> collection)
        {
            Debug.Assert(collection != null);
            this._collection = collection;
        }

        public void Dispose() => this._collection.blockReentrancyCount--;
    }
}

public readonly struct ObservableListChangedEventArgs<T>
{
    /// <summary>
    /// The action that occurred
    /// </summary>
    public ObservableListCollectionChangedAction Action { get; }

    /// <summary>
    /// The added items when <see cref="ObservableListCollectionChangedAction.Add"/> occurs or
    /// the new items when <see cref="ObservableListCollectionChangedAction.Replace"/> occurs or
    /// the moved items when <see cref="ObservableListCollectionChangedAction.Move"/> occurs
    /// </summary>
    public IReadOnlyList<T> NewItems { get; }

    /// <summary>
    /// The removed items when <see cref="ObservableListCollectionChangedAction.Remove"/> occurs or
    /// the old items when <see cref="ObservableListCollectionChangedAction.Replace"/> occurs or
    /// the moved items when <see cref="ObservableListCollectionChangedAction.Move"/> occurs or
    /// the items that were cleared when <see cref="ObservableListCollectionChangedAction.Reset"/> occurs
    /// </summary>
    public IReadOnlyList<T> OldItems { get; }

    /// <summary>
    /// The index of the 0th added, replaced or moved item(s)
    /// </summary>
    public int NewIndex { get; } = -1;

    /// <summary>
    /// The index of the 0th removed, replaced or moved item(s)
    /// </summary>
    public int OldIndex { get; } = -1;

    public ObservableListChangedEventArgs(ObservableListCollectionChangedAction action, T? oldItem, T? newItem, int oldIndex, int newIndex)
    {
        this.Action = action;
        this.OldItems = oldItem != null ? new SingletonReadOnlyList<T>(oldItem) : ReadOnlyCollection<T>.Empty;
        this.NewItems = newItem != null ? new SingletonReadOnlyList<T>(newItem) : ReadOnlyCollection<T>.Empty;
        this.NewIndex = newIndex;
        this.OldIndex = oldIndex;
    }

    public ObservableListChangedEventArgs(ObservableListCollectionChangedAction action, IEnumerable<T>? oldItems, IEnumerable<T>? newItems, int oldIndex, int newIndex)
    {
        this.Action = action;
        this.OldItems = oldItems == null ? ReadOnlyCollection<T>.Empty : (oldItems is IList<T> ? new ReadOnlyCollection<T>((IList<T>) oldItems) : oldItems.ToList().AsReadOnly());
        this.NewItems = newItems == null ? ReadOnlyCollection<T>.Empty : (newItems is IList<T> ? new ReadOnlyCollection<T>((IList<T>) newItems) : newItems.ToList().AsReadOnly());
        this.NewIndex = newIndex;
        this.OldIndex = oldIndex;
    }

    public ObservableListChangedEventArgs(IEnumerable<T>? itemsBeingCleared)
    {
        this.Action = ObservableListCollectionChangedAction.Reset;
        this.OldItems = itemsBeingCleared == null ? ReadOnlyCollection<T>.Empty : (itemsBeingCleared is IList<T> ? new ReadOnlyCollection<T>((IList<T>) itemsBeingCleared) : itemsBeingCleared.ToList().AsReadOnly());
        this.NewItems = ReadOnlyCollection<T>.Empty;
        this.OldIndex = this.NewIndex = -1;
    }
}

public enum ObservableListCollectionChangedAction
{
    /// <summary>
    /// Item(s) added. <see cref="ObservableListChangedEventArgs{T}.NewItems"/> contains the added items
    /// </summary>
    Add,

    /// <summary>
    /// Item(s) removed. <see cref="ObservableListChangedEventArgs{T}.OldItems"/> contains the removed items
    /// </summary>
    Remove,

    /// <summary>
    /// Item(s) removed. <see cref="ObservableListChangedEventArgs{T}.OldItems"/> contains the item that
    /// was removed, <see cref="ObservableListChangedEventArgs{T}.NewItems"/> contains the new item.
    /// The old and new index will be the same
    /// </summary>
    Replace,

    /// <summary>
    /// Item(s) moved. Both the old and new items lists contain the same item(s) 
    /// </summary>
    Move,

    /// <summary>
    /// Items cleared. <see cref="ObservableListChangedEventArgs{T}.OldItems"/> contains the items that were removed
    /// </summary>
    Reset
}