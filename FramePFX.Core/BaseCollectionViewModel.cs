using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FramePFX.Core.Utils;

namespace FramePFX.Core {
    public abstract class BaseCollectionViewModel<T> : BaseViewModel {
        private readonly EfficientObservableCollection<T> items;
        public ReadOnlyObservableCollection<T> Items { get; }

        public bool IsEmpty => this.items.Count < 1;

        /// <summary>
        /// Whether to use range actions or not. Range actions are not supported by some things (e.g. WPF CollectionView)
        /// <para>
        /// When false, the range actions (e.g. <see cref="AddRange"/>) will still work, but the implementation
        /// will fire a collection change event for each item in the range
        /// </para>
        /// </summary>
        protected bool UseRangeActions {
            get => this.items.UseRangeActions;
            set => this.items.UseRangeActions = value;
        }

        protected BaseCollectionViewModel() {
            this.items = new EfficientObservableCollection<T>();
            this.Items = new ReadOnlyObservableCollection<T>(this.items);
        }

        protected virtual void AddRange(IEnumerable<T> enumerable) {
            List<T> list = enumerable.ToList();
            this.items.AddRange(list);
            this.OnChildrenAddedOrRemoved(list, true);
            this.RaiseIsEmptyChanged();
        }

        protected virtual void Add(T item) {
            this.items.Add(item);
            this.RaiseIsEmptyChanged();
        }

        protected virtual void Insert(int index, T item) {
            this.items.Insert(index, item);
            this.OnChildAddedOrRemoved(item, true);
            this.RaiseIsEmptyChanged();
        }

        protected virtual void InsertRange(int index, IEnumerable<T> enumerable) {
            List<T> list = enumerable.ToList();
            this.items.InsertRange(index, list);
            this.OnChildrenAddedOrRemoved(list, true);
            this.RaiseIsEmptyChanged();
        }

        protected virtual bool Contains(T item) {
            return this.items.Contains(item);
        }

        protected virtual bool Remove(T item) {
            int index = this.IndexOf(item);
            if (index < 0) {
                return false;
            }

            this.RemoveAt(index);
            return true;
        }

        protected virtual void RemoveAll(IEnumerable<T> enumerable) {
            foreach (T item in enumerable) {
                this.Remove(item);
            }
        }

        protected virtual void RemoveAll(Predicate<T> canRemove) {
            // this.RemoveAll(this.items.Where(canRemove).ToList());
            List<T> list = this.items.ToList();
            for (int i = list.Count - 1; i >= 0; i--) {
                T item = list[i];
                if (canRemove(item)) {
                    this.OnChildAddedOrRemoved(item, false);
                    this.items.RemoveAt(i);
                }
            }

            this.RaiseIsEmptyChanged();
        }

        protected virtual int IndexOf(T item) {
            return this.items.IndexOf(item);
        }

        protected virtual void RemoveAt(int index) {
            this.OnChildAddedOrRemoved(this.items[index], false);
            this.items.RemoveAt(index);
            this.RaiseIsEmptyChanged();
        }

        protected virtual void Clear() {
            this.OnChildrenAddedOrRemoved(this.items, false);
            this.items.Clear();
            this.RaiseIsEmptyChanged();
        }

        protected virtual void RaiseIsEmptyChanged() {
            this.RaisePropertyChanged(nameof(this.IsEmpty));
        }

        protected virtual void OnChildAddedOrRemoved(T item, bool isAdded) {

        }

        protected virtual void OnChildrenAddedOrRemoved(IEnumerable<T> enumerable, bool isAdded) {
            foreach (T item in enumerable) {
                this.OnChildAddedOrRemoved(item, isAdded);
            }
        }
    }
}