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

namespace FramePFX.Utils.Collections.ObservableEx;

/// <summary>
/// A class that simplifies post-processing of an observable list for when items are removed and added
/// </summary>
public interface IObservableItemProcessorEx<out T> : IDisposable {
    event Action<T>? OnItemAdded;
    event Action<T>? OnItemRemoved;
}

/// <summary>
/// A helper class for creating observable item processors
/// </summary>
public static class ObservableItemProcessorEx {
    /// <summary>
    /// Creates a simple item processor that invokes two callbacks when an item is added to or removed from the collection 
    /// </summary>
    /// <param name="list"></param>
    /// <param name="onItemAdded"></param>
    /// <param name="onItemRemoved"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IObservableItemProcessorEx<T> MakeSimple<T>(IObservableListEx<T> list, Action<T>? onItemAdded, Action<T>? onItemRemoved) {
        return new ObservableItemProcessorExSimpleImpl<T>(list, onItemAdded, onItemRemoved);
    }

    private class ObservableItemProcessorExSimpleImpl<T> : IObservableItemProcessorEx<T> {
        private readonly IObservableListEx<T> list;

        public event Action<T>? OnItemAdded;
        public event Action<T>? OnItemRemoved;

        public ObservableItemProcessorExSimpleImpl(IObservableListEx<T> list, Action<T>? itemAdded, Action<T>? itemRemoved) {
            this.list = list ?? throw new ArgumentNullException(nameof(list));
            list.CollectionChanged += this.OnCollectionChanged;

            this.OnItemAdded = itemAdded;
            this.OnItemRemoved = itemRemoved;
        }

        private void OnCollectionChanged(IObservableListEx<T> list, ObservableListChangedEventArgs<T> e) {
            switch (e.Action) {
                case ObservableListCollectionChangedAction.Add:
                    if (this.OnItemAdded != null) {
                        foreach (T item in e.NewItems)
                            this.OnItemAdded(item);
                    }

                    break;
                case ObservableListCollectionChangedAction.Remove:
                    if (this.OnItemRemoved != null) {
                        foreach (T item in e.OldItems)
                            this.OnItemRemoved(item);
                    }

                    break;
                case ObservableListCollectionChangedAction.Replace:
                    if (this.OnItemRemoved != null) {
                        foreach (T item in e.OldItems)
                            this.OnItemRemoved(item);
                    }

                    if (this.OnItemAdded != null) {
                        foreach (T item in e.NewItems)
                            this.OnItemAdded(item);
                    }

                    break;
                case ObservableListCollectionChangedAction.Move: break;
                case ObservableListCollectionChangedAction.Reset:
                    if (this.OnItemRemoved != null) {
                        foreach (T item in e.OldItems)
                            this.OnItemRemoved(item);
                    }

                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public void Dispose() {
            this.list.CollectionChanged -= this.OnCollectionChanged;
        }
    }
}