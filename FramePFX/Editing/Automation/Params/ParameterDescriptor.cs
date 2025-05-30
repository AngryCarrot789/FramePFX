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

using System.Numerics;
using FramePFX.Editing.Automation.Keyframes;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils;

namespace FramePFX.Editing.Automation.Params;

/// <summary>
/// Used to describe information about a specific instance of an automation parameter. Inheritors should be immutable
/// </summary>
public abstract class ParameterDescriptor {
    public AutomationDataType DataType { get; }

    protected ParameterDescriptor(AutomationDataType dataType) {
        this.DataType = dataType;
    }
}

public sealed class ParameterDescriptorFloat : ParameterDescriptor, IRangedParameter<float> {
    /// <summary>
    /// The default value of the parameter
    /// </summary>
    public float DefaultValue { get; }

    /// <summary>
    /// The minimum value of the parameter. The final effective value may not drop below this
    /// </summary>
    public float Minimum { get; }

    /// <summary>
    /// The maximum value of the parameter. The final effective value may not exceed this
    /// </summary>
    public float Maximum { get; }

    public bool HasExplicitRangeLimit { get; }

    public ParameterDescriptorFloat(float defaultValue = default, float minimum = float.MinValue, float maximum = float.MaxValue) : base(AutomationDataType.Float) {
        if (minimum > maximum)
            throw new ArgumentException($"Minimum value exceeds the maximum value: {minimum} > {maximum}", nameof(minimum));
        if (defaultValue < minimum || defaultValue > maximum)
            throw new ArgumentOutOfRangeException(nameof(defaultValue), $"Default value ({defaultValue}) falls out of range of the min/max values ({minimum}/{maximum})");
        this.DefaultValue = defaultValue;
        this.Minimum = minimum;
        this.Maximum = maximum;
        this.HasExplicitRangeLimit = minimum > float.MinValue || maximum < float.MaxValue;
    }

    public float Clamp(float value) => Maths.Clamp(value, this.Minimum, this.Maximum);

    public bool IsValueOutOfRange(float value) => value < this.Minimum || value > this.Maximum;
}

public sealed class ParameterDescriptorDouble : ParameterDescriptor, IRangedParameter<double> {
    /// <summary>
    /// The default value of the parameter
    /// </summary>
    public double DefaultValue { get; }

    /// <summary>
    /// The minimum value of the parameter. The final effective value may not drop below this
    /// </summary>
    public double Minimum { get; }

    /// <summary>
    /// The maximum value of the parameter. The final effective value may not exceed this
    /// </summary>
    public double Maximum { get; }

    public bool HasExplicitRangeLimit { get; }

    public ParameterDescriptorDouble(double defaultValue = default, double minimum = double.MinValue, double maximum = double.MaxValue) : base(AutomationDataType.Double) {
        if (minimum > maximum)
            throw new ArgumentException($"Minimum value exceeds the maximum value: {minimum} > {maximum}", nameof(minimum));
        if (defaultValue < minimum || defaultValue > maximum)
            throw new ArgumentOutOfRangeException(nameof(defaultValue), $"Default value ({defaultValue}) falls out of range of the min/max values ({minimum}/{maximum})");
        this.DefaultValue = defaultValue;
        this.Minimum = minimum;
        this.Maximum = maximum;
        this.HasExplicitRangeLimit = minimum > double.MinValue || maximum < double.MaxValue;
    }

    public double Clamp(double value) => Maths.Clamp(value, this.Minimum, this.Maximum);

    public bool IsValueOutOfRange(double value) => value < this.Minimum || value > this.Maximum;
}

public sealed class ParameterDescriptorLong : ParameterDescriptor, IRangedParameter<long> {
    /// <summary>
    /// The default value of the parameter
    /// </summary>
    public long DefaultValue { get; }

    /// <summary>
    /// The minimum value of the parameter. The final effective value may not drop below this
    /// </summary>
    public long Minimum { get; }

    /// <summary>
    /// The maximum value of the parameter. The final effective value may not exceed this
    /// </summary>
    public long Maximum { get; }

    public bool HasExplicitRangeLimit { get; }

    public ParameterDescriptorLong(long defaultValue = default, long minimum = long.MinValue, long maximum = long.MaxValue) : base(AutomationDataType.Long) {
        if (minimum > maximum)
            throw new ArgumentException($"Minimum value exceeds the maximum value: {minimum} > {maximum}", nameof(minimum));
        if (defaultValue < minimum || defaultValue > maximum)
            throw new ArgumentOutOfRangeException(nameof(defaultValue), $"Default value ({defaultValue}) falls out of range of the min/max values ({minimum}/{maximum})");
        this.DefaultValue = defaultValue;
        this.Minimum = minimum;
        this.Maximum = maximum;
        this.HasExplicitRangeLimit = minimum != long.MinValue || maximum != long.MaxValue;
    }

    public long Clamp(long value) => Maths.Clamp(value, this.Minimum, this.Maximum);

    public bool IsValueOutOfRange(long value) => value < this.Minimum || value > this.Maximum;
}

public sealed class ParameterDescriptorBool : ParameterDescriptor {
    /// <summary>
    /// The default value of the parameter
    /// </summary>
    public bool DefaultValue { get; }

    public ParameterDescriptorBool(bool defaultValue = false) : base(AutomationDataType.Boolean) {
        this.DefaultValue = defaultValue;
    }
}

public sealed class ParameterDescriptorVector2 : ParameterDescriptor, IRangedParameter<Vector2> {
    /// <summary>
    /// The default value of the parameter
    /// </summary>
    public Vector2 DefaultValue { get; }

    /// <summary>
    /// The minimum value of the parameter. The final effective value may not drop below this
    /// </summary>
    public Vector2 Minimum { get; }

    /// <summary>
    /// The maximum value of the parameter. The final effective value may not exceed this
    /// </summary>
    public Vector2 Maximum { get; }

    public bool HasExplicitRangeLimit { get; }

    public ParameterDescriptorVector2() : this(default) {
    }

    public ParameterDescriptorVector2(Vector2 defaultValue) : this(defaultValue, Vectors.MinValue, Vectors.MaxValue) {
    }

    public ParameterDescriptorVector2(Vector2 defaultValue, Vector2 minimum, Vector2 maximum) : base(AutomationDataType.Vector2) {
        if (minimum.X > maximum.X || minimum.Y > maximum.Y)
            throw new ArgumentException($"Minimum value exceeds the maximum value: {minimum} > {maximum}", nameof(minimum));
        if (defaultValue.X < minimum.X || defaultValue.X > maximum.X || defaultValue.Y < minimum.Y || defaultValue.Y > maximum.Y)
            throw new ArgumentOutOfRangeException(nameof(defaultValue), $"Default value ({defaultValue}) falls out of range of the min/max values ({minimum}/{maximum})");
        this.DefaultValue = defaultValue;
        this.Minimum = minimum;
        this.Maximum = maximum;
        this.HasExplicitRangeLimit = minimum.X > float.MinValue || minimum.Y > float.MinValue || maximum.X < float.MaxValue || maximum.Y < float.MaxValue;
    }

    public Vector2 Clamp(Vector2 value) => Vector2.Clamp(value, this.Minimum, this.Maximum);

    public bool IsValueOutOfRange(Vector2 value) {
        return value.IsLessThan(this.Minimum) || value.IsGreaterThan(this.Maximum);
    }
}