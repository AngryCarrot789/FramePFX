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

using System;
using FramePFX.Editors.Automation.Params;

namespace FramePFX.Editors.DataTransfer {
    /// <summary>
    /// A main and generic implementation of <see cref="DataParameter"/>, which also provides a default value property
    /// <para>
    /// While creating derived types is not necessary, you can do so to add things like value range limits
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of value this data parameter deals with</typeparam>
    public class DataParameterGeneric<T> : DataParameter {
        private readonly ValueAccessor<T> accessor;
        protected readonly bool isObjectAccessPreferred;

        public T DefaultValue { get; }

        public DataParameterGeneric(Type ownerType, string key, ValueAccessor<T> accessor, DataParameterFlags flags = DataParameterFlags.None) : base(ownerType, key, flags) {
            if (accessor == null)
                throw new ArgumentNullException(nameof(accessor));
            this.accessor = accessor;
            this.isObjectAccessPreferred = accessor.IsObjectPreferred;
        }

        public DataParameterGeneric(Type ownerType, string key, T defaultValue, ValueAccessor<T> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, key, accessor, flags) {
            this.DefaultValue = defaultValue;
        }

        /// <summary>
        /// Gets the generic value for this parameter
        /// </summary>
        /// <param name="owner">The owner instance</param>
        /// <returns>The generic value</returns>
        public virtual T GetValue(ITransferableData owner) {
            return this.accessor.GetValue(owner);
        }

        /// <summary>
        /// Sets the generic value for this parameter
        /// </summary>
        /// <param name="owner">The owner instance</param>
        /// <param name="value">The new value</param>
        public virtual void SetValue(ITransferableData owner, T value) {
            this.OnBeginValueChange(owner);
            Exception error = null;
            try {
                this.accessor.SetValue(owner, value);
            }
            catch (Exception e) {
                error = e;
            }
            finally {
                this.OnEndValueChangeHelper(owner, ref error);
            }
        }

        public override object GetObjectValue(ITransferableData owner) {
            return this.accessor.GetObjectValue(owner);
        }

        public override void SetObjectValue(ITransferableData owner, object value) {
            this.OnBeginValueChange(owner);
            Exception error = null;
            try {
                this.accessor.SetObjectValue(owner, value);
            }
            catch (Exception e) {
                error = e;
            }
            finally {
                this.OnEndValueChangeHelper(owner, ref error);
            }
        }
    }
}