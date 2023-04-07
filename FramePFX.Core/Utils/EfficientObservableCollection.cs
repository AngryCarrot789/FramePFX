using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace MCNBTViewer.Core.Utils {
    public class EfficientObservableCollection<T> : ObservableCollection<T> {
        /// <summary>
        /// Whether to use range actions or not. Range actions are not supported by some things (e.g. WPF CollectionView)
        /// <para>
        /// When false, the range actions (e.g. <see cref="AddRange"/>) will still work, but the implementation
        /// will fire a collection change event for each item in the range
        /// </para>
        /// </summary>
        public bool UseRangeActions { get; set; }

        public EfficientObservableCollection() {

        }

        public EfficientObservableCollection(IEnumerable<T> collection) : base(collection) {

        }

        public EfficientObservableCollection(List<T> list) : base(list) {

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

            if (this.UseRangeActions) {
                this.InsertRangeInternal(index, list);
                this.OnCountPropertyChanged();
                this.OnIndexerPropertyChanged();
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list, index));
            }
            else {
                for (int i = 0; i < list.Count; i++) {
                    T value = list[i];
                    int j = index + i;
                    this.Items.Insert(j, value);
                    this.OnCountPropertyChanged();
                    this.OnIndexerPropertyChanged();
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, j));
                }
            }
        }

        private void InsertRangeInternal(int index, IEnumerable<T> items) {
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

        public void ClearAndAddRange(IEnumerable<T> enumerable) {
            this.CheckReentrancy();
            this.Items.Clear();
            this.InsertRangeInternal(0, enumerable);
            this.OnCountPropertyChanged();
            this.OnIndexerPropertyChanged();
            this.OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
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

        public bool RemoveFirst(Predicate<T> canRemove) {
            this.CheckReentrancy();
            IList<T> list = this.Items;
            for (int i = 0; i < list.Count; i++) {
                T value = this.Items[i];
                if (!canRemove(value)) {
                    continue;
                }

                this.RemoveAt(i);
                return true;
            }

            return false;
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
    }
}