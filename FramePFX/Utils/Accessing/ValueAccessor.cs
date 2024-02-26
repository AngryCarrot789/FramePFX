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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

namespace FramePFX.Utils.Accessing {
    /// <summary>
    /// A delegate for a method that gets a value from a target object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public delegate T AccessGetter<out T>(object a);

    /// <summary>
    /// A delegate for a method that sets a value to the given value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public delegate void AccessSetter<in T>(object a, T v);

    /// <summary>
    /// A class used by parameters (and data parameters) to get and set the effective value of a specific parameter for an object
    /// </summary>
    /// <typeparam name="TValue">The type of value this accessor accesses</typeparam>
    public abstract class ValueAccessor<TValue> {
        /// <summary>
        /// Returns true when the boxed getter and setters are preferred, e.g. this instance is reflection-based which always uses boxed values
        /// </summary>
        public bool IsObjectPreferred { get; protected set; }

        /// <summary>
        /// Gets the generic value
        /// </summary>
        public abstract TValue GetValue(object owner);

        /// <summary>
        /// Gets the object value
        /// </summary>
        public abstract object GetObjectValue(object owner);

        /// <summary>
        /// Sets the generic value
        /// </summary>
        public abstract void SetValue(object owner, TValue value);

        /// <summary>
        /// Sets the object value
        /// </summary>
        public abstract void SetObjectValue(object owner, object value);
    }
}