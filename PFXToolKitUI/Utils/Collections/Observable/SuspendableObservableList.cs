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

namespace PFXToolKitUI.Utils.Collections.Observable;

/// <summary>
/// An observable list that supports suspending a single collection change event until
/// the dispatch token is disposed, in which case, the event is then invoked
/// </summary>
public class SuspendableObservableList<T> : ObservableList<T> {
    private int suspendEventsCount;
    private BaseEvent? scheduledEvent;

    protected override void OnItemsAdded(int index, IList<T> items) {
        if (this.suspendEventsCount > 0)
            this.scheduledEvent = this.scheduledEvent != null ? throw new InvalidOperationException("Event already scheduled") : new EventItemsAdded(index, items);
        else
            base.OnItemsAdded(index, items);
    }

    protected override void OnItemsRemoved(int index, IList<T> items) {
        if (this.suspendEventsCount > 0)
            this.scheduledEvent = this.scheduledEvent != null ? throw new InvalidOperationException("Item removed already scheduled") : new EventItemsRemoved(index, items);
        else
            base.OnItemsRemoved(index, items);
    }

    protected override void OnItemReplaced(int index, T oldItem, T newItem) {
        if (this.suspendEventsCount > 0)
            this.scheduledEvent = this.scheduledEvent != null ? throw new InvalidOperationException("Item Replace already scheduled") : new EventItemReplaced(index, oldItem, newItem);
        else
            base.OnItemReplaced(index, oldItem, newItem);
    }

    protected override void OnItemMoved(int oldIndex, int newIndex, T item) {
        if (this.suspendEventsCount > 0)
            this.scheduledEvent = this.scheduledEvent != null ? throw new InvalidOperationException("Item move already scheduled") : new EventItemMoved(oldIndex, newIndex, item);
        else
            base.OnItemMoved(oldIndex, newIndex, item);
    }

    public SuspendEventToken SuspendEvents() => new SuspendEventToken(this);

    public struct SuspendEventToken : IDisposable {
        private SuspendableObservableList<T> list;

        public SuspendEventToken(SuspendableObservableList<T> list) {
            this.list = list;
            list.suspendEventsCount++;
        }

        public void Dispose() {
            if (this.list == null)
                return;

            if (--this.list.suspendEventsCount == 0)
                this.list.DispatchEvents();
            this.list = null;
        }
    }

    private void DispatchEvents() {
        BaseEvent? e = this.scheduledEvent;
        this.scheduledEvent = null;
        e?.Dispatch(this);
    }

    private abstract class BaseEvent {
        public abstract void Dispatch(SuspendableObservableList<T> list);
    }

    private class EventItemsAdded : BaseEvent {
        public readonly int index;
        public readonly IList<T> items;

        public EventItemsAdded(int index, IList<T> items) {
            this.index = index;
            this.items = items;
        }

        public override void Dispatch(SuspendableObservableList<T> list) {
            list.OnItemsAdded(this.index, this.items);
        }
    }

    private class EventItemsRemoved : BaseEvent {
        public readonly int index;
        public readonly IList<T> items;

        public EventItemsRemoved(int index, IList<T> items) {
            this.index = index;
            this.items = items;
        }

        public override void Dispatch(SuspendableObservableList<T> list) {
            list.OnItemsRemoved(this.index, this.items);
        }
    }

    private class EventItemReplaced : BaseEvent {
        public readonly int index;
        public readonly T oldItem, newItem;

        public EventItemReplaced(int index, T oldItem, T newItem) {
            this.index = index;
            this.oldItem = oldItem;
            this.newItem = newItem;
        }

        public override void Dispatch(SuspendableObservableList<T> list) {
            list.OnItemReplaced(this.index, this.oldItem, this.newItem);
        }
    }

    private class EventItemMoved : BaseEvent {
        public readonly int oldIndex, newIndex;
        public readonly T item;

        public EventItemMoved(int oldIndex, int newIndex, T item) {
            this.oldIndex = oldIndex;
            this.newIndex = newIndex;
            this.item = item;
        }

        public override void Dispatch(SuspendableObservableList<T> list) {
            list.OnItemMoved(this.oldIndex, this.newIndex, this.item);
        }
    }
}