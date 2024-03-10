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

namespace FramePFX.Behaviours
{
    public class BehaviourCollection : FreezableCollection<BehaviourBase>
    {
        public static readonly DependencyProperty BehavioursProperty = DependencyProperty.RegisterAttached("Behaviours", typeof(BehaviourCollection), typeof(BehaviourCollection), new PropertyMetadata(null, OnBehavioursChanged));

        private static readonly Action<Visual> RemoveHandler;
        private static readonly Action<Visual> AddHandler;
        private bool IsOwnerVisual;

        /// <summary>
        /// Gets the element that owns this collection
        /// </summary>
        public DependencyObject Owner { get; private set; }

        // lazily add/remove event handler for VAC, as handlers existing do have some tiny overhead in the VT operations
        private int vacCount;

        public BehaviourCollection()
        {
            ((INotifyCollectionChanged) this).CollectionChanged += this.OnCollectionChanged;
        }

        static BehaviourCollection()
        {
            VisualAncestorChangedEventInterface.CreateInterface(OnVisualAncestorChanged, out AddHandler, out RemoveHandler);
        }

        internal void RegisterVAC(BehaviourBase behaviour)
        {
            if (this.Owner == null)
                throw new InvalidOperationException("No owner");

            if (!this.IsOwnerVisual)
            {
                Debug.WriteLine(behaviour.GetType() + " tried to register the VisualAncestorChanged event, but our owner is not a visual: " + this.Owner);
                return;
            }

            if (this.vacCount == 0)
            {
                AddHandler((Visual) this.Owner);
            }

            this.vacCount++;
        }

        internal void UnregisterVAC()
        {
            if (this.Owner == null)
                throw new InvalidOperationException("No owner");
            if (!this.IsOwnerVisual)
                return;

            if (--this.vacCount == 0)
            {
                RemoveHandler((Visual) this.Owner);
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    this.DetachAndTryAttachAll(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    DetachAll(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    DetachAll(e.OldItems);
                    this.DetachAndTryAttachAll(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Move: break;
                case NotifyCollectionChangedAction.Reset:
                    DetachAll(this);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void DetachAndTryAttachAll(IEnumerable enumerable)
        {
            foreach (BehaviourBase behaviour in enumerable)
            {
                if (behaviour.AttachedElement != null)
                    behaviour.Detach();
                if (this.Owner != null && ((IBehaviour) behaviour).CanAttachTo(this.Owner))
                    behaviour.Attach(this);
            }
        }

        private static void DetachAll(IEnumerable enumerable)
        {
            foreach (BehaviourBase behaviour in enumerable)
            {
                if (behaviour.AttachedElement != null)
                    behaviour.Detach();
            }
        }

        public void Connect(DependencyObject element)
        {
            if (this.Owner != null)
                throw new InvalidOperationException("Already attached");
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            this.Owner = element;
            this.IsOwnerVisual = element is Visual;
            this.DetachAndTryAttachAll(this);
        }

        public void Disconnect()
        {
            if (this.Owner == null)
                throw new InvalidOperationException("Not attached: no owner");

            DetachAll(this);
            if (this.IsOwnerVisual && this.vacCount > 0)
            {
                Debug.WriteLine("Expected VACCount to be zero when all items are detached");
                Debugger.Break();
                this.vacCount = 0;
                RemoveHandler((Visual) this.Owner);
            }

            this.Owner = null;
            this.IsOwnerVisual = false;
        }

        public static void SetBehaviours(UIElement element, BehaviourCollection value)
        {
            element.SetValue(BehavioursProperty, value);
        }

        public static BehaviourCollection GetBehaviours(UIElement element)
        {
            return (BehaviourCollection) element.GetValue(BehavioursProperty);
        }

        private static void OnBehavioursChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is BehaviourCollection oldCollection)
            {
                oldCollection.Disconnect();
            }

            if (e.NewValue is BehaviourCollection newCollection)
            {
                if (newCollection.Owner != null)
                    newCollection.Disconnect();
                newCollection.Connect(d);
            }
        }

        private static void OnVisualAncestorChanged(DependencyObject element, DependencyObject oldParent)
        {
            if (element.GetValue(BehavioursProperty) is BehaviourCollection collection)
            {
                // this should be non-null realistically
                if (collection.Owner != element)
                {
                    Debug.WriteLine("Fatal error: received VAC event for unrelated visual");
                    Debugger.Break();
                    return;
                }

                foreach (BehaviourBase behaviour in collection)
                {
                    BehaviourBase.InternalProcessVisualParentChanged(behaviour, oldParent);
                }
            }
        }
    }
}