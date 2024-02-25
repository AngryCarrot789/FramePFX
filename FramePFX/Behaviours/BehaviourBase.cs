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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace FramePFX.Behaviours {
    /// <summary>
    /// The base class for behaviours. Provides functionality to be associated with a control.
    /// This class should not be inherited directly, instead, use <see cref="Behaviour{T}"/>
    /// </summary>
    public abstract class BehaviourBase : Freezable, IBehaviour {
        private static readonly Dictionary<Type, bool> HasVisualParentChangedOverridden;
        private static readonly Type[] VPCArgs = new Type[] {typeof(DependencyObject)};

        protected readonly bool CanProcessVisualParentChanged;
        private DependencyObject element;

        public DependencyObject AttachedElement => this.element;

        /// <summary>
        /// Gets the behaviour collection that owns this behaviour, or null
        /// </summary>
        public BehaviourCollection Collection { get; private set; }

        protected BehaviourBase() {
            this.CanProcessVisualParentChanged = GetHasVisualParentChangedHandler(this.GetType());
        }

        static BehaviourBase() {
            HasVisualParentChangedOverridden = new Dictionary<Type, bool>();
        }

        /// <summary>
        /// Called when this behaviour is attached to an applicable target element
        /// </summary>
        protected abstract void OnAttached();

        /// <summary>
        /// Called when this behaviour is detached from its target element
        /// </summary>
        protected abstract void OnDetatched();

        public void Attach(BehaviourCollection collection, DependencyObject newElement) {
            if (this.element != null)
                throw new InvalidOperationException("Already attached to another element: " + this.element);
            if (newElement == null)
                throw new ArgumentNullException(nameof(newElement));
            if (!this.CanAttachToType(newElement))
                throw new ArgumentException("Target element is incompatible");

            this.Collection = collection;
            this.element = newElement;
            if (this.CanProcessVisualParentChanged)
                collection.RegisterVAC(this);

            try {
                this.OnAttached();
            }
            catch (Exception e) {
                if (this.CanProcessVisualParentChanged)
                    collection.UnregisterVAC();
                this.element = null;
                this.Collection = null;
                throw new Exception("Failed to call " + nameof(this.OnAttached), e);
            }
        }

        public void Detatch() {
            if (this.element == null)
                throw new InvalidOperationException("Cannot detach from nothing; we are not attached");

            if (this.CanProcessVisualParentChanged)
                this.Collection.UnregisterVAC();

            try {
                this.OnDetatched();
            }
            catch (Exception e) {
                throw new Exception("Failed to call " + nameof(this.OnDetatched), e);
            }
            finally {
                this.element = null;
            }
        }

        /// <summary>
        /// Invoked when our attached element's visual parent changes. Overriding the method enables the
        /// internal mechanisms for this method to be called, because processing parent changed adds a
        /// tiny bit of UI overhead.
        /// <para>
        /// Only the old parent is provided. The new parent can be accessed via the visual tree utils class
        /// </para>
        /// </summary>
        /// <param name="oldParent">Our element's previous parent</param>
        protected virtual void OnVisualParentChanged(DependencyObject oldParent) {

        }

        protected override Freezable CreateInstanceCore() {
            return (Freezable) Activator.CreateInstance(this.GetType());
        }

        bool IBehaviour.CanAttachTo(DependencyObject targetType) => this.CanAttachToType(targetType);

        protected abstract bool CanAttachToType(DependencyObject target);

        internal static void InternalProcessVisualParentChanged(BehaviourBase behaviour, DependencyObject oldParent) {
            if (behaviour.CanProcessVisualParentChanged)
                behaviour.OnVisualParentChanged(oldParent);
        }

        private static bool GetHasVisualParentChangedHandler(Type type) {
            if (!HasVisualParentChangedOverridden.TryGetValue(type, out bool value)) {
                HasVisualParentChangedOverridden[type] = value = GetRawHasVisualParentChangedHandler(type);
            }

            return value;
        }

        private static bool GetRawHasVisualParentChangedHandler(Type type) {
            MethodInfo method = type.GetMethod(nameof(OnVisualParentChanged), BindingFlags.Instance | BindingFlags.NonPublic, null, CallingConventions.HasThis, VPCArgs, null);
            if (!ReferenceEquals(method, null)) {
                MethodInfo baseMethod = method.GetBaseDefinition();
                return baseMethod.DeclaringType != method.DeclaringType;
            }

            Debug.WriteLine(nameof(OnVisualParentChanged) + " did not exist");
            Debugger.Break();
            return false;
        }
    }
}