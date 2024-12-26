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

using System.Numerics;
using FramePFX.Utils;
using FramePFX.Utils.Accessing;

namespace FramePFX.DataTransfer;

/// <summary>
/// A <see cref="DataParameter{T}"/> that manages an Vector2, which is two 32-bit single precision floating
/// point numbers (aka, a float). This also has an optional minimum and maximum value range
/// </summary>
public sealed class DataParameterVector2 : DataParameter<Vector2>, IRangedParameter<Vector2> {
    public static Vector2 Vector2MinValue => new Vector2(float.MinValue, float.MinValue);
    public static Vector2 Vector2MaxValue => new Vector2(float.MaxValue, float.MaxValue);

    public Vector2 Minimum { get; }
    public Vector2 Maximum { get; }
    public bool HasExplicitRangeLimit { get; }

    public DataParameterVector2(Type ownerType, string name, ValueAccessor<Vector2> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, name, default, accessor, flags) {
    }

    public DataParameterVector2(Type ownerType, string name, Vector2 defValue, ValueAccessor<Vector2> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, name, defValue, Vector2MinValue, Vector2MaxValue, accessor, flags) {
    }

    public DataParameterVector2(Type ownerType, string name, Vector2 defValue, Vector2 minValue, Vector2 maxValue, ValueAccessor<Vector2> accessor, DataParameterFlags flags = DataParameterFlags.None) : base(ownerType, name, defValue, accessor, flags) {
        if (minValue.X > maxValue.X || minValue.Y > maxValue.Y)
            throw new ArgumentException($"All or a part of the Minimum value exceeds the Maximum value: {minValue} > {maxValue}", nameof(minValue));
        if (defValue.X < minValue.X || defValue.Y < minValue.Y || defValue.X > maxValue.X || defValue.Y > maxValue.Y)
            throw new ArgumentOutOfRangeException(nameof(defValue), $"Default value ({defValue}) falls out of range of the min/max values ({minValue}/{maxValue})");
        this.Minimum = minValue;
        this.Maximum = maxValue;
        this.HasExplicitRangeLimit = minValue.X > float.MinValue || minValue.Y > float.MinValue || maxValue.X < float.MaxValue || maxValue.Y < float.MaxValue;
    }

    public Vector2 Clamp(Vector2 value) => new Vector2(Maths.Clamp(value.X, this.Minimum.X, this.Maximum.X), Maths.Clamp(value.Y, this.Minimum.Y, this.Maximum.Y));

    public bool IsValueOutOfRange(Vector2 value) => value.X < this.Minimum.X || value.Y < this.Minimum.Y || value.X > this.Maximum.X || value.Y > this.Maximum.Y;

    public override void SetValue(ITransferableData owner, Vector2 value) {
        if (this.HasExplicitRangeLimit) {
            value = this.Clamp(value);
        }

        base.SetValue(owner, value);
    }

    public override void SetObjectValue(ITransferableData owner, object? value) {
        if (this.HasExplicitRangeLimit) {
            Vector2 unboxed = (Vector2) value!;
            Vector2 clamped = this.Clamp(unboxed);
            if (!Maths.Equals(unboxed, clamped))
                value = clamped;
        }

        base.SetObjectValue(owner, value);
    }
}