using System;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Utils;

namespace FramePFX.Editors.Automation.Params {
    /// <summary>
    /// Used to describe information about a specific instance of an automation parameter. Inheritors should be immutable
    /// </summary>
    public abstract class ParameterDescriptor {
        public AutomationDataType DataType { get; }

        protected ParameterDescriptor(AutomationDataType dataType) {
            this.DataType = dataType;
        }
    }

    public sealed class ParameterDescriptorFloat : ParameterDescriptor {
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

        public ParameterDescriptorFloat(float defaultValue = default, float minimum = float.MinValue, float maximum = float.MaxValue) : base(AutomationDataType.Float) {
            if (minimum > maximum)
                throw new ArgumentException($"Minimum value exceeds the maximum value: {minimum} > {maximum}", nameof(minimum));
            if (defaultValue < minimum || defaultValue > maximum)
                throw new ArgumentOutOfRangeException(nameof(defaultValue), $"Default value ({defaultValue}) falls out of range of the min/max values ({minimum}/{maximum})");
            this.DefaultValue = defaultValue;
            this.Minimum = minimum;
            this.Maximum = maximum;
        }

        public float Clamp(float value) => Maths.Clamp(value, this.Minimum, this.Maximum);
    }

    public sealed class ParameterDescriptorDouble : ParameterDescriptor {
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

        public ParameterDescriptorDouble(double defaultValue = default, double minimum = double.MinValue, double maximum = double.MaxValue) : base(AutomationDataType.Double) {
            if (minimum > maximum)
                throw new ArgumentException($"Minimum value exceeds the maximum value: {minimum} > {maximum}", nameof(minimum));
            if (defaultValue < minimum || defaultValue > maximum)
                throw new ArgumentOutOfRangeException(nameof(defaultValue), $"Default value ({defaultValue}) falls out of range of the min/max values ({minimum}/{maximum})");
            this.DefaultValue = defaultValue;
            this.Minimum = minimum;
            this.Maximum = maximum;
        }

        public double Clamp(double value) => Maths.Clamp(value, this.Minimum, this.Maximum);
    }

    public sealed class ParameterDescriptorLong : ParameterDescriptor {
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

        public ParameterDescriptorLong(long defaultValue = default, long minimum = long.MinValue, long maximum = long.MaxValue) : base(AutomationDataType.Long) {
            if (minimum > maximum)
                throw new ArgumentException($"Minimum value exceeds the maximum value: {minimum} > {maximum}", nameof(minimum));
            if (defaultValue < minimum || defaultValue > maximum)
                throw new ArgumentOutOfRangeException(nameof(defaultValue), $"Default value ({defaultValue}) falls out of range of the min/max values ({minimum}/{maximum})");
            this.DefaultValue = defaultValue;
            this.Minimum = minimum;
            this.Maximum = maximum;
        }

        public long Clamp(long value) => Maths.Clamp(value, this.Minimum, this.Maximum);
    }

    public sealed class ParameterDescriptorBoolean : ParameterDescriptor {
        /// <summary>
        /// The default value of the parameter
        /// </summary>
        public bool DefaultValue { get; }

        public ParameterDescriptorBoolean(bool defaultValue = false) : base(AutomationDataType.Boolean) {
            this.DefaultValue = defaultValue;
        }
    }
}