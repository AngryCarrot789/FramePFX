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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FramePFX.Utils.Collections {
    public enum ListChangeEventType {
        /// <summary>
        /// Represents an insertion event. <see cref="ListChangedEventArgs.NewIndex"/> contains the insertion index
        /// </summary>
        Insert,
        Remove,
        Replace,
        Move,
        Clear
    }

    public readonly struct ListChangedEventArgs<T> {
        /// <summary>
        /// The event type
        /// </summary>
        public readonly ListChangeEventType EventType;

        /// <summary>
        /// The index of the item during a remove, replace or move event
        /// </summary>
        public readonly int OldIndex;

        /// <summary>
        /// The insertion index, or destination index during a replace or move event
        /// </summary>
        public readonly int NewIndex;

        /// <summary>
        /// Gets the previous value. This is only valid during a <see cref="ListChangeEventType.Remove"/> or <see cref="ListChangeEventType.Replace"/>
        /// </summary>
        public readonly T OldValue;

        /// <summary>
        /// Gets the value associated with this event. This is only valid during <see cref="ListChangeEventType.Insert"/> and <see cref="ListChangeEventType.Replace"/>
        /// </summary>
        public readonly T NewValue;

        public ListChangedEventArgs(ListChangeEventType eventType, int oldIndex, int newIndex, T oldValue, T newValue) {
            this.EventType = eventType;
            this.OldIndex = oldIndex;
            this.NewIndex = newIndex;
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }
    }

    public delegate void ListChangedEventHandler<T>(object sender, ListChangedEventArgs<T> args);

    public interface INotifyCollectionChangedEx<T> {
        event ListChangedEventHandler<T> ListChanged;
    }

    public class ObservableList<T> : Collection<T>, INotifyCollectionChangedEx<T> {
        private int collectionChangeLoopCount;

        public event ListChangedEventHandler<T> ListChanged;

        public ObservableList() {
        }

        public ObservableList(List<T> list) : base(list != null ? new List<T>(list.Count) : throw new ArgumentNullException(nameof(list))) {
            this.CopyFrom(list);
        }

        public ObservableList(IEnumerable<T> collection) {
            this.CopyFrom(collection ?? throw new ArgumentNullException(nameof(collection)));
        }

        private void CopyFrom(IEnumerable<T> collection) {
            if (collection == null)
                return;
            IList<T> items = this.Items;
            foreach (T obj in collection)
                items.Add(obj);
        }

        public void Move(int oldIndex, int newIndex) => this.MoveItem(oldIndex, newIndex);

        protected override void ClearItems() {
            this.CheckReentrancy();
            base.ClearItems();
            this.OnCollectionChanged(new ListChangedEventArgs<T>(ListChangeEventType.Clear, -1, -1, default, default));
        }

        protected override void RemoveItem(int index) {
            this.CheckReentrancy();
            T obj = this[index];
            base.RemoveItem(index);
            this.OnCollectionChanged(new ListChangedEventArgs<T>(ListChangeEventType.Remove, index, -1, obj, default));
        }

        protected override void InsertItem(int index, T item) {
            this.CheckReentrancy();
            base.InsertItem(index, item);
            this.OnCollectionChanged(new ListChangedEventArgs<T>(ListChangeEventType.Insert, -1, index, default, item));
        }

        protected override void SetItem(int index, T item) {
            this.CheckReentrancy();
            T obj = this[index];
            base.SetItem(index, item);
            this.OnCollectionChanged(new ListChangedEventArgs<T>(ListChangeEventType.Replace, -1, index, obj, item));
        }

        protected virtual void MoveItem(int oldIndex, int newIndex) {
            this.CheckReentrancy();
            T obj = this[oldIndex];
            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, obj);
            this.OnCollectionChanged(new ListChangedEventArgs<T>(ListChangeEventType.Move, oldIndex, newIndex, default, default));
        }

        protected virtual void OnCollectionChanged(ListChangedEventArgs<T> e) {
            if (this.ListChanged == null)
                return;
            try {
                this.collectionChangeLoopCount++;
                this.ListChanged?.Invoke(this, e);
            }
            finally {
                this.collectionChangeLoopCount--;
            }
        }

        protected void CheckReentrancy() {
            if (this.collectionChangeLoopCount > 0 && this.ListChanged != null && this.ListChanged.GetInvocationList().Length > 1)
                throw new InvalidOperationException("Cannot modify collection during a collection change event");
        }
    }

    public class ReadOnlyObservableList<T> : ReadOnlyCollection<T>, INotifyCollectionChangedEx<T> {
        public event ListChangedEventHandler<T> ListChanged;

        public ReadOnlyObservableList(ObservableList<T> list) : base(list) {
            list.ListChanged += this.OnListCollectionChanged;
        }

        private void OnListCollectionChanged(object sender, ListChangedEventArgs<T> args) {
            this.ListChanged?.Invoke(this, args);
        }
    }
}