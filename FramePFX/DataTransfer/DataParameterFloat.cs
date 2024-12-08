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

/// <summary>
/// A <see cref="DataParameter{T}"/> that manages a 32-bit single precision floating point
/// number (aka, a float). This also has an optional minimum and maximum value range
/// </summary>
public sealed class DataParameterFloat : DataParameter<float>, IRangedParameter<float>
{
    public float Minimum { get; }
    public float Maximum { get; }
    public bool HasExplicitRangeLimit { get; }

    public DataParameterFloat(Type ownerType, string name, ValueAccessor<float> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, name, 0.0F, accessor, flags) {
    }

    public DataParameterFloat(Type ownerType, string name, float defValue, ValueAccessor<float> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, name, defValue, float.MinValue, float.MaxValue, accessor, flags) {
    }

    public DataParameterFloat(Type ownerType, string name, float defValue, float minValue, float maxValue, ValueAccessor<float> accessor, DataParameterFlags flags = DataParameterFlags.None) : base(ownerType, name, defValue, accessor, flags)
    {
        if (minValue > maxValue)
            throw new ArgumentException($"Minimum value exceeds the maximum value: {minValue} > {maxValue}", nameof(minValue));
        if (defValue < minValue || defValue > maxValue)
            throw new ArgumentOutOfRangeException(nameof(defValue), $"Default value ({defValue}) falls out of range of the min/max values ({minValue}/{maxValue})");
        this.Minimum = minValue;
        this.Maximum = maxValue;
        this.HasExplicitRangeLimit = minValue > float.MinValue || maxValue < float.MaxValue;
    }

    public float Clamp(float value) => Maths.Clamp(value, this.Minimum, this.Maximum);

    public bool IsValueOutOfRange(float value) => value < this.Minimum || value > this.Maximum;

    public override void SetValue(ITransferableData owner, float value)
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
            float unboxed = (float) value!;
            float clamped = Maths.Clamp(unboxed, this.Minimum, this.Maximum);
            if (!Maths.Equals(unboxed, clamped))
                value = clamped;
        }

        base.SetObjectValue(owner, value);
    }
}