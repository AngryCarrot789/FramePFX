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

namespace FramePFX.Behaviours
{
    /// <summary>
    /// The main class for all behaviours. Behaviours can be used to add functionality to a class without having
    /// to use behind-code or create a sub-class of an existing control (e.g. a behaviour that processes buttons
    /// clicks, changes a control's background based on context data, or anything else really)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Behaviour<T> : BehaviourBase where T : DependencyObject
    {
        public new T AttachedElement => (T) ((IBehaviour) this).AttachedElement;

        static Behaviour()
        {
        }

        protected sealed override bool CanAttachToType(DependencyObject target) => target is T;
    }
}