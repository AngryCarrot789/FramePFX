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

namespace PFXToolKitUI.Utils.Collections.Observable;

/// <summary>
/// An optimised observable collection. This class is not thread safe; manual synchronization required
/// </summary>
/// <typeparam name="T">Type of value to store</typeparam>
public class ObservableList<T> : Collection<T>, IObservableList<T> {
    private readonly List<T> myItems;
    private SimpleMonitor? _monitor;
    private readonly bool isDerivedType;
    protected int blockReentrancyCount;

    public event ObservableListMultipleItemsEventHandler<T>? ItemsAdded;
    public event ObservableListMultipleItemsEventHandler<T>? ItemsRemoved;
    public event ObservableListReplaceEventHandler<T>? ItemReplaced;
    public event ObservableListSingleItemEventHandler<T>? ItemMoved;


    public ObservableList() : this(new List<T>()) {
    }

    public ObservableList(IEnumerable<T> collection) : this(new List<T>(collection ?? throw new ArgumentNullException(nameof(collection)))) {
    }

    public ObservableList(List<T> list) : base(new List<T>(list ?? throw new ArgumentNullException(nameof(list)))) {
        this.myItems = (List<T>?) base.Items!;

        // Optimisation
        // MethodInfo info = this.GetType().GetMethod(nameof(this.OnItemsRemoved), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)!;
        // this.IsItemsRemovedOverridden = info.GetBaseDefinition().DeclaringType != info.DeclaringType;
        // If we are a derived type, then disable optimisations that avoid invoking specific
        // notification methods since they may involve copying the lots of items when it's unnecessary
        this.isDerivedType = this.GetType() != typeof(ObservableList<T>);
    }

    protected override void InsertItem(int index, T item) {
        this.CheckReentrancy();
        this.myItems.Insert(index, item);

        // Invoke base method when derived or we have an ItemsAdded handler
        if (this.isDerivedType || this.ItemsAdded != null)
            this.OnItemsAdded(index, new SingletonList<T>(item));
    }

    public void AddRange(IEnumerable<T> items) => this.InsertRange(this.Count, items);

    public void AddSpanRange(ReadOnlySpan<T> items) => this.InsertSpanRange(this.Count, items);

    public void InsertRange(int index, IEnumerable<T> items) {
        this.CheckReentrancy();

        // Slight risk in passing list to the ItemsAdded event in case items mutates asynchronously... meh
        if (items is IList<T> list) {
            this.myItems.InsertRange(index, list);
        }
        else {
            // Probably enumerator method or something along those lines, convert to list for speedy insertion
            list = items.ToList();
            this.myItems.InsertRange(index, list);
        }

        if (this.isDerivedType || this.ItemsAdded != null) // Stops the event handler modifying the list
            list = list.AsReadOnly();

        this.OnItemsAdded(index, list);
    }

    /// <summary>
    /// Inserts a span of items into this list. Before trying to optimise code to use this, know that this method may
    /// create an array and then list from the span if we are a derived type or we have event handlers for <see cref="ItemsAdded"/>
    /// </summary>
    /// <param name="index"></param>
    /// <param name="items"></param>
    public void InsertSpanRange(int index, ReadOnlySpan<T> items) {
        this.CheckReentrancy();

        this.myItems.InsertRange(index, items);
        if (this.isDerivedType || this.ItemsAdded != null)
            this.OnItemsAdded(index, new ReadOnlyCollection<T>(items.ToArray()));
    }

    protected override void RemoveItem(int index) {
        this.CheckReentrancy();
        T removedItem = this[index];
        this.myItems.RemoveAt(index);

        // Invoke base method when derived or we have an ItemsRemoved handler
        if (this.isDerivedType || this.ItemsRemoved != null)
            this.OnItemsRemoved(index, new SingletonList<T>(removedItem));
    }

    public void RemoveRange(int index, int count) {
        this.CheckReentrancy();
        if (!this.isDerivedType && this.ItemsRemoved == null) {
            // We are not a derived type, and we have no ItemsRemoved handler,
            // so we don't need to create any pointless sub-lists
            this.myItems.RemoveRange(index, count);
        }
        else {
            List<T> items = this.myItems.Slice(index, count);
            this.myItems.RemoveRange(index, count);
            this.OnItemsRemoved(index, items.AsReadOnly());
        }
    }

    protected override void SetItem(int index, T newItem) {
        this.CheckReentrancy();
        T oldItem = this[index];
        base.SetItem(index, newItem);
        this.OnItemReplaced(index, oldItem, newItem);
    }

    public void Move(int oldIndex, int newIndex) => this.MoveItem(oldIndex, newIndex);

    protected virtual void MoveItem(int oldIndex, int newIndex) {
        this.CheckReentrancy();
        T item = this[oldIndex];
        base.RemoveItem(oldIndex);
        base.InsertItem(newIndex, item);
        this.OnItemMoved(oldIndex, newIndex, item);
    }

    protected override void ClearItems() {
        this.CheckReentrancy();
        if (this.myItems.Count < 1) {
            return;
        }

        if (!this.isDerivedType && this.ItemsRemoved == null) {
            // We are not a derived type, and we have no ItemsRemoved handler,
            // so we don't need to create any pointless sub-lists
            this.myItems.Clear();
        }
        else {
            ReadOnlyCollection<T> items = this.myItems.ToList().AsReadOnly();
            this.myItems.Clear();
            this.OnItemsRemoved(0, items);
        }
    }

    protected virtual void OnItemsAdded(int index, IList<T> items) {
        try {
            this.blockReentrancyCount++;
            this.ItemsAdded?.Invoke(this, items, index);
        }
        finally {
            this.blockReentrancyCount--;
        }
    }

    protected virtual void OnItemsRemoved(int index, IList<T> items) {
        try {
            this.blockReentrancyCount++;
            this.ItemsRemoved?.Invoke(this, items, index);
        }
        finally {
            this.blockReentrancyCount--;
        }
    }

    protected virtual void OnItemReplaced(int index, T oldItem, T newItem) {
        try {
            this.blockReentrancyCount++;
            this.ItemReplaced?.Invoke(this, oldItem, newItem, index);
        }
        finally {
            this.blockReentrancyCount--;
        }
    }

    protected virtual void OnItemMoved(int oldIndex, int newIndex, T item) {
        try {
            this.blockReentrancyCount++;
            this.ItemMoved?.Invoke(this, item, oldIndex, newIndex);
        }
        finally {
            this.blockReentrancyCount--;
        }
    }

    public IDisposable BlockReentrancy() {
        this.blockReentrancyCount++;
        return this.EnsureMonitorInitialized();
    }

    protected void CheckReentrancy() {
        if (this.blockReentrancyCount > 0) {
            // we can allow changes if there's only one listener - the problem
            // only arises if reentrant changes make the original event args
            // invalid for later listeners.  This keeps existing code working
            // (e.g. Selector.SelectedItems).
            if (this.ItemsAdded?.GetInvocationList().Length > 1 ||
                this.ItemsRemoved?.GetInvocationList().Length > 1 ||
                this.ItemReplaced?.GetInvocationList().Length > 1 ||
                this.ItemMoved?.GetInvocationList().Length > 1)
                throw new InvalidOperationException("Reentrancy Not Allowed");
        }
    }

    private SimpleMonitor EnsureMonitorInitialized() => this._monitor ??= new SimpleMonitor(this);

    private sealed class SimpleMonitor : IDisposable {
        internal int _busyCount; // Only used during (de)serialization to maintain compatibility with desktop. Do not rename (binary serialization)

        [NonSerialized] internal ObservableList<T> _collection;

        public SimpleMonitor(ObservableList<T> collection) {
            Debug.Assert(collection != null);
            this._collection = collection;
        }

        public void Dispose() => this._collection.blockReentrancyCount--;
    }

    public static void Test() {
        ObservableList<int> list = new ObservableList<int>();

        // Indexable processor removes back to front as an optimisation, can disable in constructor
        ObservableItemProcessor.MakeIndexable(list, (s, i, o) => {
            Console.WriteLine($"Added '{o}' at {i}");
        }, (s, i, o) => {
            Console.WriteLine($"Removed '{o}' at {i}");
        }, (s, oldI, newI, o) => {
            Console.WriteLine($"Moved '{o}' from {oldI} to {newI}");
        });

        list.Add(0);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        list.Add(4);
        list.Add(5);
        list.Add(6);
        list.Add(7);
        list.Add(8);

        // list = 0,1,2,3,4,5,6,7,8
        // Removing 4 items at index 2 removes 2,3,4,5
        // Remaining list = 0,1,6,7,8
        list.RemoveRange(2, 4);

        // assert list.Count == 5
        // assert list == [0,1,6,7,8]
    }
}