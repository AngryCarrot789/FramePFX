using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace FramePFX.Core.Utils {
    /// <summary>
    /// An extended version of <see cref="ObservableCollection{T}"/> which supports ranged action (optional; can be disabled/enabled
    /// due to the fact that WPF's collection views don't like range actions), and also supports some helper functions (e.g. <see cref="ClearAndAdd"/>)
    ///
    /// </summary>
    /// <typeparam name="T">Type of object to store</typeparam>
    public class ObservableCollectionEx<T> : ObservableCollection<T> {
        /// <summary>
        /// Whether to use range actions or not. Range actions are not supported by some things (e.g. WPF CollectionView)
        /// <para>
        /// When false, the range actions (e.g. <see cref="AddRange"/>) will still work, but the implementation
        /// will fire a collection change event for each item in the range
        /// </para>
        /// </summary>
        public bool UseRangeActions { get; set; }

        public ObservableCollectionEx() {
        }

        public ObservableCollectionEx(IEnumerable<T> collection) : base(collection) {
        }

        public ObservableCollectionEx(List<T> list) : base(list) {
        }

        public TResult FirstOfType<TResult>() {
            foreach (T value in this.Items) {
                if (value is TResult result) {
                    return result;
                }
            }

            return default;
        }

        public bool FirstOfType<TResult>(out TResult result) {
            foreach (T value in this.Items) {
                if (value is TResult r) {
                    result = r;
                    return true;
                }
            }

            result = default;
            return false;
        }

        public int FindIndexOf(Predicate<T> matchFunction) {
            IList<T> list = this.Items;
            for (int i = 0; i < list.Count; i++) {
                T value = this.Items[i];
                if (matchFunction(value)) {
                    return i;
                }
            }

            return -1;
        }

        public int FindIndexOfReverse(Predicate<T> matchFunction) {
            IList<T> list = this.Items;
            for (int i = list.Count - 1; i >= 0; i++) {
                T value = this.Items[i];
                if (matchFunction(value)) {
                    return i;
                }
            }

            return -1;
        }

        public void AddRange(IEnumerable<T> collection) {
            this.InsertRange(this.Count, collection);
        }

        public void InsertRange(int index, IEnumerable<T> items) {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index > this.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (items is ICollection<T> collection && collection.Count < 1) {
                return;
            }

            this.CheckReentrancy();
            if (!(items is IList<T> list))
                list = items.ToList(); // Prevent multiple enumeration of items by creating list
            this.InsertRangeInternal(index, list);
        }

        private void InsertRangeInternalRangedUnsafe(int index, IEnumerable<T> items) {
            if (this.Items is List<T> rawList) {
                rawList.InsertRange(index, items);
            }
            else {
                int i = index;
                foreach (T value in items) {
                    this.Items.Insert(i++, value);
                }
            }
        }

        private void InsertRangeInternalNonRanged(int index, IEnumerable<T> items) {
            int i = index;
            foreach (T value in items) {
                this.Items.Insert(i, value);
                this.OnCountPropertyChanged();
                this.OnIndexerPropertyChanged();
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, i));
                i++;
            }
        }

        private void InsertRangeInternal(int index, IEnumerable<T> items) {
            if (this.UseRangeActions) {
                this.InsertRangeInternalRangedUnsafe(index, items);
                this.OnCollectionReset();
            }
            else {
                this.InsertRangeInternalNonRanged(index, items);
            }
        }

        public void ClearAndAdd(T value) {
            this.CheckReentrancy();
            if (this.UseRangeActions) {
                this.Items.Clear();
                this.Items.Add(value);
                this.OnCollectionReset();
            }
            else {
                this.ClearItems();
                this.Add(value);
            }
        }

        public void ClearAndAddRange(IEnumerable<T> enumerable) {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));
            if (this.UseRangeActions) {
                this.CheckReentrancy();
                this.Items.Clear();
                this.InsertRangeInternalNonRanged(0, enumerable);
            }
            else {
                this.ClearItems();
                this.InsertRangeInternalNonRanged(0, enumerable);
            }
        }

        public void RemoveRange(int index, int count) {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative");

            IList<T> list = this.Items;
            int endIndex = index + count;
            if (endIndex > list.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Too many elements to remove");
            this.CheckReentrancy();
            if (this.UseRangeActions) {
                List<T> changed = new List<T>(count); // this.Items.Skip(index).Take(count).ToList();
                for (int i = index; i < endIndex; i++) {
                    changed.Add(list[i]);
                }

                if (list is List<T> rawList) {
                    rawList.RemoveRange(index, count);
                }
                else {
                    // Remove backwards to hopefully keep the array copying per RemoveAt call constant
                    for (int i = endIndex; i >= index; i--) {
                        list.RemoveAt(i);
                    }
                }

                this.OnCountPropertyChanged();
                this.OnIndexerPropertyChanged();
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changed, index));
            }
            else {
                for (int i = endIndex; i >= index; i--) {
                    T value = list[i];
                    list.RemoveAt(i);
                    this.OnCountPropertyChanged();
                    this.OnIndexerPropertyChanged();
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value, index));
                }
            }
        }

        public void RemoveRange(IEnumerable<T> items) {
            if (items == null)
                throw new ArgumentNullException(nameof(items), "Items cannot be null");

            IList<T> list = this.Items;
            this.CheckReentrancy();
            if (ReferenceEquals(items, this) || ReferenceEquals(items, list)) {
                items = items.ToList();
            }

            foreach (T item in items) {
                int index = this.Items.IndexOf(item);
                if (index == -1) {
                    continue;
                }

                list.RemoveAt(index);
                this.OnCountPropertyChanged();
                this.OnIndexerPropertyChanged();
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            }
        }

        /// <summary>
        /// Finds the first item that matches the given predicate, then removes it
        /// </summary>
        /// <param name="canRemove">The predicate to match items</param>
        /// <param name="reverse">Whether to search from the end of the list to the start, instead of start to end (a performance helper parameter)</param>
        /// <returns>True if the item was removed, otherwise false</returns>
        public bool RemoveFirst(Predicate<T> canRemove, bool reverse = false) {
            this.CheckReentrancy();
            int index = reverse ? this.FindIndexOfReverse(canRemove) : this.FindIndexOf(canRemove);
            if (index == -1)
                return false;
            this.RemoveAt(index);
            return true;
        }

        public void OrderByCollection(IEnumerable<T> enumerable) {
            int i = 0;
            foreach (T item in enumerable) {
                this.Move(this.IndexOf(item), i);
                i++;
            }
        }

        /// <summary>
        /// Helper to raise a PropertyChanged event for the Count property
        /// </summary>
        private void OnCountPropertyChanged() => this.OnPropertyChanged(EventArgsCache.CountPropertyChanged);

        /// <summary>
        /// Helper to raise a PropertyChanged event for the Indexer property
        /// </summary>
        private void OnIndexerPropertyChanged() => this.OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);

        private void OnCollectionReset() {
            this.OnCountPropertyChanged();
            this.OnIndexerPropertyChanged();
            this.OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
        }

        public IEnumerable<T> ReverseEnumerable() => this.Items.Reverse();
    }
}