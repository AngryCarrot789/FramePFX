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
using FramePFX.Utils.Visuals;

namespace FramePFX.Behaviours
{
    public abstract class BehaviourEx<T> : Behaviour<T> where T : DependencyObject
    {
        private readonly RoutedEventHandler loadedStateChangedHandler;
        private bool isAttachedEx;
        private bool isAttachedCore;

        protected BehaviourEx()
        {
            this.loadedStateChangedHandler = (s, e) =>
            {
                this.UpdateAttachedState();
            };
        }

        protected sealed override void OnAttached()
        {
            if (this.AttachedElement is FrameworkElement control)
            {
                control.Loaded += this.loadedStateChangedHandler;
                control.Unloaded += this.loadedStateChangedHandler;
            }

            this.isAttachedCore = true;
            this.UpdateAttachedState();
        }

        protected sealed override void OnDetached()
        {
            if (this.AttachedElement is FrameworkElement control)
            {
                control.Loaded -= this.loadedStateChangedHandler;
                control.Unloaded -= this.loadedStateChangedHandler;
            }

            this.isAttachedCore = false;
            this.UpdateAttachedState();
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent) => this.UpdateAttachedState();

        private void UpdateAttachedState()
        {
            DependencyObject parent = VisualTreeUtils.GetParent(this.AttachedElement);
            if (!this.isAttachedCore || parent == null || this.AttachedElement is FrameworkElement control && !control.IsLoaded)
            {
                if (this.isAttachedEx)
                    this.DetachEx();
            }
            else
            {
                if (!this.isAttachedEx)
                    this.AttachEx();
            }
        }

        private void AttachEx()
        {
            if (this.isAttachedEx)
                throw new InvalidOperationException("Extended functionality is already attached");

            this.isAttachedEx = true;
            this.OnAttachedEx();
        }

        private void DetachEx()
        {
            if (!this.isAttachedEx)
                throw new InvalidOperationException("Extended functionality is not attached");

            try
            {
                this.OnDetachedEx();
            }
            finally
            {
                this.isAttachedEx = false;
            }
        }

        /// <summary>
        /// Called when this behaviour becomes attached or the element is added to the visual tree
        /// </summary>
        protected abstract void OnAttachedEx();

        /// <summary>
        /// Called when this behaviour becomes detached or the element is removed from the visual tree
        /// </summary>
        protected abstract void OnDetachedEx();
    }
}