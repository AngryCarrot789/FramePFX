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

using System.Windows;

namespace FramePFX.Behaviours {
    public interface IBehaviour {
        // /// <summary>
        // /// Returns true if this behaviour processes when its attached visual parent changes
        // /// </summary>
        // bool CanProcessVisualParentChanged { get; }

        /// <summary>
        /// Gets our attached element, or null
        /// </summary>
        DependencyObject AttachedElement { get; }

        /// <summary>
        /// Gets the behaviour collection that owns this behaviour, or null
        /// </summary>
        BehaviourCollection Collection { get; }

        /// <summary>
        /// Attempts to attach this behaviour to the element
        /// </summary>
        /// <param name="collection">The new collection owner</param>
        /// <param name="newElement">The attached element</param>
        /// <exception cref="InvalidOperationException">We are already attached</exception>
        /// <exception cref="ArgumentException">The element is not applicable (see <see cref="CanAttachTo"/>)</exception>
        /// <exception cref="Exception">An exception was thrown while processing the <see cref="BehaviourBase.OnAttached"/> method</exception>
        void Attach(BehaviourCollection collection, DependencyObject newElement);

        /// <summary>
        /// Detaches from our current element
        /// </summary>
        /// <exception cref="InvalidOperationException">We are already attached</exception>
        /// <exception cref="Exception">An exception was thrown while processing the <see cref="BehaviourBase.OnAttached"/> method</exception>
        void Detatch();

        /// <summary>
        /// Checks if the given targetType is applicable to be attached
        /// </summary>
        /// <param name="targetType">The type wanting to be attached</param>
        /// <returns>A bool...</returns>
        bool CanAttachTo(DependencyObject targetType);
    }
}