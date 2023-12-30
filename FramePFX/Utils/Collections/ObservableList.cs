using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace FramePFX.Utils.Collections {
    public enum ListChangeMode {
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
        /// The event mode
        /// </summary>
        public readonly ListChangeMode Mode;

        /// <summary>
        /// The removal index, replace index, or source index during a move event
        /// </summary>
        public readonly int OldIndex;

        /// <summary>
        /// The insertion index, replace index, or destination index during a move event
        /// </summary>
        public readonly int NewIndex;

        /// <summary>
        /// Gets the value associated with this event. This is only valid during <see cref="ListChangeMode.Insert"/> and <see cref="ListChangeMode.Replace"/>
        /// </summary>
        public readonly T Value;

        public ListChangedEventArgs(ListChangeMode mode, int oldIndex, int newIndex, T value) {
            this.Mode = mode;
            this.OldIndex = oldIndex;
            this.NewIndex = newIndex;
            this.Value = value;
        }

        public ListChangedEventArgs(NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    this.Mode = ListChangeMode.Insert;
                    this.OldIndex = -1;
                    this.NewIndex = e.NewStartingIndex;
                    this.Value = (T) e.NewItems[0];
                    break;
                case NotifyCollectionChangedAction.Move:
                    this.Mode = ListChangeMode.Move;
                    this.OldIndex = e.OldStartingIndex;
                    this.NewIndex = e.NewStartingIndex;
                    this.Value = (T) e.NewItems[0];
                    break;
                case NotifyCollectionChangedAction.Remove:
                    this.Mode = ListChangeMode.Remove;
                    this.OldIndex = e.OldStartingIndex;
                    this.NewIndex = -1;
                    this.Value = default;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    this.Mode = ListChangeMode.Replace;
                    this.OldIndex = e.OldStartingIndex;
                    this.NewIndex = e.NewStartingIndex;
                    this.Value = (T) e.NewItems[0];
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.Mode = ListChangeMode.Clear;
                    this.OldIndex = -1;
                    this.NewIndex = -1;
                    this.Value = default;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }

    public delegate void ListChangedEventHandler<T>(object sender, ListChangedEventArgs<T> args);

    public interface INotifyCollectionChangedEx<T> {
        event ListChangedEventHandler<T> ListChanged;
    }

    public class ObservableList<T> : Collection<T>, INotifyPropertyChanged, INotifyCollectionChanged, INotifyCollectionChangedEx<T> {
        private const string CountString = "Count";
        private const string IndexerName = "Item[]";
        private int collectionChangeLoopCount;

        public event ListChangedEventHandler<T> ListChanged;

        public ObservableList() {
        }

        public ObservableList(List<T> list) : base(list != null ? new List<T>(list.Count) : throw new ArgumentNullException(nameof(list))) {
            this.CopyFrom(list);
        }

        public ObservableList(IEnumerable<T> collection) {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            this.CopyFrom(collection);
        }

        private void CopyFrom(IEnumerable<T> collection) {
            IList<T> items = this.Items;
            if (collection == null)
                return;
            foreach (T obj in collection)
                items.Add(obj);
        }

        public void Move(int oldIndex, int newIndex) => this.MoveItem(oldIndex, newIndex);

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged {
            add => this.PropertyChanged += value;
            remove => this.PropertyChanged -= value;
        }

        [field: NonSerialized]
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        protected override void ClearItems() {
            this.CheckReentrancy();
            base.ClearItems();
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionReset();
        }

        protected override void RemoveItem(int index) {
            this.CheckReentrancy();
            T obj = this[index];
            base.RemoveItem(index);
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, obj, index);
        }

        protected override void InsertItem(int index, T item) {
            this.CheckReentrancy();
            base.InsertItem(index, item);
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        protected override void SetItem(int index, T item) {
            this.CheckReentrancy();
            T obj = this[index];
            base.SetItem(index, item);
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Replace, obj, item, index);
        }

        protected virtual void MoveItem(int oldIndex, int newIndex) {
            this.CheckReentrancy();
            T obj = this[oldIndex];
            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, obj);
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Move, obj, newIndex, oldIndex);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) {
            this.PropertyChanged?.Invoke(this, e);
        }

        protected virtual event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            if (this.CollectionChanged == null && this.ListChanged == null)
                return;
            try {
                this.collectionChangeLoopCount++;
                this.CollectionChanged?.Invoke(this, e);
                this.ListChanged?.Invoke(this, new ListChangedEventArgs<T>(e));
            }
            finally {
                this.collectionChangeLoopCount--;
            }
        }

        protected void CheckReentrancy() {
            if (this.collectionChangeLoopCount > 0 && this.CollectionChanged != null && this.CollectionChanged.GetInvocationList().Length > 1)
                throw new InvalidOperationException("Cannot modify collection during a collection change event");
        }

        private void OnPropertyChanged(string propertyName) => this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index) => this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex) {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index) {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        private void OnCollectionReset() => this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}