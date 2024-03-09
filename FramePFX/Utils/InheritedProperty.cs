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

namespace FramePFX.Utils
{
    /// <summary>
    /// A property that can inherit a value from a parent instance
    /// </summary>
    /// <typeparam name="T">The type of value to store</typeparam>
    public class InheritedProperty<T>
    {
        private readonly InheritedProperty<T> parent;
        private T internalValue;

        /// <summary>
        /// Gets the current value (if <see cref="HasLocalValue"/> is true) or the parent's value, or sets the current value and marks <see cref="HasLocalValue"/> as true
        /// </summary>
        public T Value {
            get => this.HasLocalValue ? this.internalValue : (this.parent != null ? this.parent.Value : default);
            set
            {
                this.HasLocalValue = true;
                this.internalValue = value;
            }
        }

        /// <summary>
        /// Whether or not this current instance has a value set or not
        /// </summary>
        public bool HasLocalValue { get; private set; }

        public InheritedProperty()
        {
        }

        public InheritedProperty(T value)
        {
            this.Value = value;
        }

        public InheritedProperty(InheritedProperty<T> parent)
        {
            this.parent = parent;
        }

        public InheritedProperty(InheritedProperty<T> parent, T value)
        {
            this.parent = parent;
            this.Value = value;
        }

        public bool GetValue(out T value)
        {
            if (this.HasLocalValue)
            {
                value = this.internalValue;
                return true;
            }

            if (this.parent != null && this.parent.GetValue(out value))
            {
                return true;
            }

            value = default;
            return false;
        }

        public void Clear()
        {
            this.HasLocalValue = false;
            this.internalValue = default;
        }
    }
}