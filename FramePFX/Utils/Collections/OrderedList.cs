using System;
using System.Collections;
using System.Collections.Generic;

namespace FramePFX.Utils.Collections {
    public class OrderedList<T> : IList<T> {
        private readonly List<T> list;
        private readonly Comparison<T> comparison;

        public OrderedList(Comparison<T> comparison) {
            this.list = new List<T>();
            this.comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
        }

        public List<T>.Enumerator GetEnumerator() => this.list.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.list.GetEnumerator();

        public void Add(T item) {
            int index = CollectionUtils.GetSortInsertionIndex(this.list, item, this.comparison);
            this.list.Insert(index, item);
        }

        public void Clear() => this.list.Clear();

        public bool Contains(T item) {
            return this.IndexOf(item) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex) {
            this.list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            return this.list.Remove(item);
        }

        public int Count => this.list.Count;

        public bool IsReadOnly => false;

        public int IndexOf(T item) {
            return this.list.IndexOf(item);
        }

        public void Insert(int index, T item) {
            this.list.Insert(index, item);
        }

        public void RemoveAt(int index) {
            this.list.RemoveAt(index);
        }

        public T this[int index] {
            get => this.list[index];
            set => this.list[index] = value;
        }
    }
}