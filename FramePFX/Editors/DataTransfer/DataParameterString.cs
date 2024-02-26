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
using FramePFX.Utils;
using FramePFX.Utils.Accessing;

namespace FramePFX.Editors.DataTransfer {
    /// <summary>
    /// A <see cref="DataParameter{T}"/> that manages a string value, and provides a character limit (both upper and lower limits)
    /// </summary>
    public sealed class DataParameterString : DataParameter<string> {
        private readonly bool hasCharLimit;

        /// <summary>
        /// The smallest number of characters allowed. Default is 0
        /// </summary>
        public int MinimumChars { get; }

        /// <summary>
        /// The maximum number of characters allowed in the string. Default is <see cref="int.MaxValue"/>, meaning effectively unlimited
        /// </summary>
        public int MaximumChars { get; }

        public DataParameterString(Type ownerType, string key, ValueAccessor<string> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, key, null, accessor, flags) {
        }

        public DataParameterString(Type ownerType, string key, string defValue, ValueAccessor<string> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, key, defValue, 0, int.MaxValue, accessor, flags) {
        }

        public DataParameterString(Type ownerType, string key, string defValue, int minChars, int maxChars, ValueAccessor<string> accessor, DataParameterFlags flags = DataParameterFlags.None) : base(ownerType, key, defValue, accessor, flags) {
            if (minChars > maxChars)
                throw new ArgumentException($"Minimum value exceeds the maximum value: {minChars} > {maxChars}", nameof(minChars));
            this.hasCharLimit = minChars != 0 || maxChars != int.MinValue;
            if (this.hasCharLimit && (defValue == null || defValue.Length < minChars || defValue.Length > maxChars))
                throw new ArgumentOutOfRangeException(nameof(defValue), $"Default value ({defValue})'s length falls out of range of the min/max values ({minChars}/{maxChars})");

            this.MinimumChars = minChars;
            this.MaximumChars = maxChars;
        }

        public override void SetValue(ITransferableData owner, string value) {
            if (this.hasCharLimit)
                value = value.FitLength(this.MinimumChars);
            base.SetValue(owner, value);
        }

        public override void SetObjectValue(ITransferableData owner, object value) {
            base.SetObjectValue(owner, this.CoerceValue((string) value));
        }

        private string CoerceValue(string value) {
            if (this.hasCharLimit) {
                if (value == null || value.Length < this.MinimumChars) {
                    if (value == null) {
                        value = this.DefaultValue;
                    }
                    else {
                        value += StringUtils.Repeat(' ', this.MinimumChars - value.Length);
                    }
                }
                else if (value.Length > this.MaximumChars) {
                    value = value.Substring(0, this.MaximumChars);
                }
            }

            return value;
        }
    }
}