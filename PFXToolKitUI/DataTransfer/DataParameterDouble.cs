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

using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Accessing;

namespace PFXToolKitUI.DataTransfer;

/// <summary>
/// A <see cref="DataParameter{T}"/> that manages a 64-bit double precision floating point
/// number (aka, a double). This also has an optional minimum and maximum value range
/// </summary>
public sealed class DataParameterDouble : DataParameter<double>, IRangedParameter<double> {
    public double Minimum { get; }
    public double Maximum { get; }
    public bool HasExplicitRangeLimit { get; }

    public DataParameterDouble(Type ownerType, string name, ValueAccessor<double> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, name, 0.0, accessor, flags) {
    }

    public DataParameterDouble(Type ownerType, string name, double defValue, ValueAccessor<double> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, name, defValue, double.MinValue, double.MaxValue, accessor, flags) {
    }

    public DataParameterDouble(Type ownerType, string name, double defValue, double minValue, double maxValue, ValueAccessor<double> accessor, DataParameterFlags flags = DataParameterFlags.None) : base(ownerType, name, defValue, accessor, flags) {
        if (minValue > maxValue)
            throw new ArgumentException($"Minimum value exceeds the maximum value: {minValue} > {maxValue}", nameof(minValue));
        if (defValue < minValue || defValue > maxValue)
            throw new ArgumentOutOfRangeException(nameof(defValue), $"Default value ({defValue}) falls out of range of the min/max values ({minValue}/{maxValue})");
        this.Minimum = minValue;
        this.Maximum = maxValue;
        this.HasExplicitRangeLimit = minValue > double.MinValue || maxValue < double.MaxValue;
    }

    public double Clamp(double value) => Maths.Clamp(value, this.Minimum, this.Maximum);

    public bool IsValueOutOfRange(double value) => value < this.Minimum || value > this.Maximum;

    public override void SetValue(ITransferableData owner, double value) {
        if (this.HasExplicitRangeLimit) {
            value = Maths.Clamp(value, this.Minimum, this.Maximum);
        }

        base.SetValue(owner, value);
    }

    public override void SetObjectValue(ITransferableData owner, object? value) {
        if (this.HasExplicitRangeLimit) {
            double unboxed = (double) value;
            double clamped = Maths.Clamp(unboxed, this.Minimum, this.Maximum);
            if (!DoubleUtils.AreClose(unboxed, clamped))
                value = clamped;
        }

        base.SetObjectValue(owner, value);
    }
}