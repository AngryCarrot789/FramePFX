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
using System.Windows;
using System.Windows.Media;
using FramePFX.Editors.Commands;
using FramePFX.Utils.Visuals;

namespace FramePFX.Behaviours {
    /// <summary>
    /// The base class for behaviours. Provides functionality to be associated with a control
    /// </summary>
    public class BehaviourBase : Freezable {
        public static readonly DependencyProperty BehavioursProperty = DependencyProperty.RegisterAttached("Behaviours", typeof(BehaviourCollection), typeof(BehaviourBase), new PropertyMetadata(null, OnBehavioursChanged));

        private static readonly Action<Visual> RemoveHandler;
        private static readonly Action<Visual> AddHandler;
        private readonly bool canProcessVisualParentChanged;

        public UIElement Element { get; private set; }

        public BehaviourCollection Collection { get; private set; }

        public BehaviourBase(bool canProcessVisualParentChanged = false) {
            this.canProcessVisualParentChanged = canProcessVisualParentChanged;
        }

        protected virtual void OnAttached() {
        }

        protected virtual void OnDetatched() {
        }

        public void Attach(BehaviourCollection collection, UIElement element) {
            if (this.Element != null)
                throw new InvalidOperationException("Already attached to another element: " + this.Element);

            this.Collection = collection;
            this.Element = element;
            if (this.canProcessVisualParentChanged)
                collection.RegisterVAC();

            try {
                this.OnAttached();
            }
            catch (Exception e) {
                if (this.canProcessVisualParentChanged)
                    collection.UnregisterVAC();
                this.Element = null;
                this.Collection = null;
                throw new Exception("Failed to call " + nameof(this.OnAttached), e);
            }
        }

        public void Detatch() {
            if (this.Element == null)
                throw new InvalidOperationException("Cannot detach from nothing; we are not attached");

            if (this.canProcessVisualParentChanged)
                this.Collection.UnregisterVAC();

            try {
                this.OnDetatched();
            }
            catch (Exception e) {
                throw new Exception("Failed to call " + nameof(this.OnDetatched), e);
            }
            finally {
                this.Element = null;
            }
        }

        protected internal virtual void OnVisualParentChanged() {
        }

        public static void SetBehaviours(UIElement element, BehaviourCollection value) {
            element.SetValue(BehavioursProperty, value);
        }

        public static BehaviourCollection GetBehaviours(UIElement element) {
            return (BehaviourCollection) element.GetValue(BehavioursProperty);
        }

        private static void OnBehavioursChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is BehaviourCollection oldCollection) {
                oldCollection.Disconnect();
            }

            if (e.NewValue is BehaviourCollection newCollection && d is UIElement element) {
                newCollection.Connect(element);
            }
        }

        protected override Freezable CreateInstanceCore() => throw new InvalidOperationException("Cannot create clone");

        private void CheckIsAttached() {
            if (this.Element == null)
                throw new InvalidOperationException("Cannot perform operation while no element is attached");
        }
    }
}