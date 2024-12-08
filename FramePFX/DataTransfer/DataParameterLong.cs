// 
// Copyright (c) 2024-2024 REghZy
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

using FramePFX.Utils;
using FramePFX.Utils.Accessing;

namespace FramePFX.DataTransfer;

public sealed class DataParameterLong : DataParameter<long>, IRangedParameter<long>
{
    public long Minimum { get; }
    public long Maximum { get; }
    public bool HasExplicitRangeLimit { get; }

    public DataParameterLong(Type ownerType, string name, ValueAccessor<long> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, name, 0L, accessor, flags) {
    }

    public DataParameterLong(Type ownerType, string name, long defValue, ValueAccessor<long> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, name, defValue, long.MinValue, long.MaxValue, accessor, flags) {
    }

    public DataParameterLong(Type ownerType, string name, long defValue, long minValue, long maxValue, ValueAccessor<long> accessor, DataParameterFlags flags = DataParameterFlags.None) : base(ownerType, name, defValue, accessor, flags)
    {
        if (minValue > maxValue)
            throw new ArgumentException($"Minimum value exceeds the maximum value: {minValue} > {maxValue}", nameof(minValue));
        if (defValue < minValue || defValue > maxValue)
            throw new ArgumentOutOfRangeException(nameof(defValue), $"Default value ({defValue}) falls out of range of the min/max values ({minValue}/{maxValue})");
        this.Minimum = minValue;
        this.Maximum = maxValue;
        this.HasExplicitRangeLimit = minValue != long.MinValue || maxValue != long.MaxValue;
    }

    public long Clamp(long value) => this.HasExplicitRangeLimit ? Maths.Clamp(value, this.Minimum, this.Maximum) : value;

    public bool IsValueOutOfRange(long value) => this.HasExplicitRangeLimit && (value < this.Minimum || value > this.Maximum);

    public override void SetValue(ITransferableData owner, long value)
    {
        if (this.HasExplicitRangeLimit)
        {
            value = Maths.Clamp(value, this.Minimum, this.Maximum);
        }

        base.SetValue(owner, value);
    }

    public override void SetObjectValue(ITransferableData owner, object? value)
    {
        if (this.HasExplicitRangeLimit)
        {
            long unboxed = (long) value;
            long clamped = Maths.Clamp(unboxed, this.Minimum, this.Maximum);
            if (unboxed != clamped)
            {
                value = clamped;
            }
        }

        base.SetObjectValue(owner, value);
    }
}