using System;
using System.Numerics;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Automation.Keys {
    public abstract class KeyDescriptor {

    }

    public class KeyDescriptorFloat : KeyDescriptor {
        public float DefaultValue { get; }
        public float Minimum { get; }
        public float Maximum { get; }
        public int Precision { get; }
        public float Step { get; }

        public bool HasPrecision => this.Precision >= 0;

        public bool IsStepEnabled => !float.IsNaN(this.Step);

        public KeyDescriptorFloat(float defaultValue, float minimum = float.NegativeInfinity, float maximum = float.PositiveInfinity, int precision = -1, float step = float.NaN) {
            this.DefaultValue = defaultValue;
            this.Minimum = minimum;
            this.Maximum = maximum;
            this.Precision = precision;
            this.Step = step;
        }

        public float Clamp(float value) {
            value = Maths.Clamp(value, this.Minimum, this.Maximum);
            return this.Precision >= 0 ? (float) Math.Round(value, this.Precision) : value;
        }
    }

    public class KeyDescriptorDouble : KeyDescriptor {
        public double DefaultValue { get; }
        public double Minimum { get; }
        public double Maximum { get; }
        public int Precision { get; }
        public double Step { get; }

        public bool HasPrecision => this.Precision >= 0;

        public bool IsStepEnabled => !double.IsNaN(this.Step);

        public KeyDescriptorDouble(double defaultValue, double minimum = double.NegativeInfinity, double maximum = double.PositiveInfinity, int precision = -1, double step = double.NaN) {
            this.DefaultValue = defaultValue;
            this.Minimum = minimum;
            this.Maximum = maximum;
            this.Precision = precision;
            this.Step = step;
        }

        public double Clamp(double value) {
            value = Maths.Clamp(value, this.Minimum, this.Maximum);
            return this.Precision >= 0 ? Math.Round(value, this.Precision) : value;
        }
    }

    public class KeyDescriptorLong : KeyDescriptor {
        public long DefaultValue { get; }
        public long Minimum { get; }
        public long Maximum { get; }
        public long Step { get; }

        public bool HasStep => this.Step > 1;

        public KeyDescriptorLong(long defaultValue, long minimum = long.MinValue, long maximum = long.MaxValue, long step = 1) {
            if (step < 1)
                throw new ArgumentOutOfRangeException(nameof(step), "Step must be greater than zero");
            if (defaultValue < minimum || defaultValue > maximum)
                throw new ArgumentOutOfRangeException(nameof(defaultValue), "Default value must be between minimum and maximum");
            this.DefaultValue = defaultValue;
            this.Minimum = minimum;
            this.Maximum = maximum;
            this.Step = step;
        }

        public long Clamp(long value) { // TODO: implement round to nearest step using modulo maybe?
            return Maths.Clamp(value, this.Minimum, this.Maximum);
        }
    }

    public class KeyDescriptorBoolean : KeyDescriptor {
        public bool DefaultValue { get; }

        public KeyDescriptorBoolean(bool defaultValue = false) {
            this.DefaultValue = defaultValue;
        }
    }

    public class KeyDescriptorVector2 : KeyDescriptor {
        public Vector2 DefaultValue { get; }
        public Vector2 Minimum { get; }
        public Vector2 Maximum { get; }
        public int Precision { get; }

        public bool HasPrecision => this.Precision >= 0;

        public KeyDescriptorVector2(Vector2 defaultValue, Vector2 minimum, Vector2 maximum, int precision = -1) {
            this.DefaultValue = defaultValue;
            this.Minimum = minimum;
            this.Maximum = maximum;
            this.Precision = precision;
        }

        public Vector2 Clamp(Vector2 value) {
            value = value.Clamp(this.Minimum, this.Maximum);
            return this.Precision >= 0 ? value.Round(this.Precision) : value;
        }
    }
}