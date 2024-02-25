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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using FramePFX.Utils.Visuals;

namespace FramePFX.Behaviours {
    public class BehaviourCollection : FreezableCollection<BehaviourBase> {
        private static readonly Action<Visual> RemoveHandler;
        private static readonly Action<Visual> AddHandler;

        public UIElement Owner { get; private set; }

        // lazily add/remove event handler for VAC, as handlers existing do have some tiny overhead in the VT operations
        private int vacCount;

        public BehaviourCollection() {
            ((INotifyCollectionChanged) this).CollectionChanged += this.OnCollectionChanged;
        }

        static BehaviourCollection() {
            VisualAncestorChangedEventInterface.CreateInterface(OnVisualAncestorChanged, out AddHandler, out RemoveHandler);
        }

        internal void RegisterVAC() {
            if (this.Owner == null) {
                throw new InvalidOperationException("No owner");
            }

            if (this.vacCount == 0) {
                AddHandler(this.Owner);
            }

            this.vacCount++;
        }

        internal void UnregisterVAC() {
            if (this.Owner == null) {
                throw new InvalidOperationException("No owner");
            }

            if (--this.vacCount == 0) {
                RemoveHandler(this.Owner);
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    this.DetatchAndTryAttachAll(e.OldItems, this.Owner);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    DetatchAll(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    DetatchAll(e.OldItems);
                    this.DetatchAndTryAttachAll(e.NewItems, this.Owner);
                    break;
                case NotifyCollectionChangedAction.Move: break;
                case NotifyCollectionChangedAction.Reset:
                    DetatchAll(this);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void DetatchAndTryAttachAll(IEnumerable enumerable, UIElement element) {
            foreach (BehaviourBase behaviour in enumerable) {
                if (behaviour.Element != null)
                    behaviour.Detatch();
                if (element != null)
                    behaviour.Attach(this, element);
            }
        }

        private static void DetatchAll(IEnumerable enumerable) {
            foreach (BehaviourBase behaviour in enumerable) {
                if (behaviour.Element != null)
                    behaviour.Detatch();
            }
        }

        public void Connect(UIElement element) {
            if (this.Owner != null) {
                throw new InvalidOperationException("Already attached");
            }

            this.Owner = element ?? throw new ArgumentNullException(nameof(element));
            this.DetatchAndTryAttachAll(this, element);
        }

        public void Disconnect() {
            if (this.Owner == null) {
                throw new InvalidOperationException("Not attached: no owner");
            }

            DetatchAll(this);

            if (this.vacCount > 0) {
                Debug.WriteLine("Expected VACCount to be zero when all items are detached");
                Debugger.Break();
                this.vacCount = 0;
                RemoveHandler(this.Owner);
            }

            this.Owner = null;
        }

        private static void OnVisualAncestorChanged(DependencyObject element, DependencyObject oldParent) {
            if (element.GetValue(BehaviourBase.BehavioursProperty) is BehaviourCollection collection) {
                if (collection.Owner != element) {
                    Debugger.Break();
                    Debug.WriteLine("Fatal error: received VAC event for unrelated visual");
                    return;
                }

                foreach (BehaviourBase behaviour in collection) {
                    behaviour.OnVisualParentChanged();
                }
            }
        }
    }
}