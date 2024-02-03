using System;
using FramePFX.Editors.Automation.Params;
using FramePFX.Utils;

namespace FramePFX.Editors.DataTransfer {
    /// <summary>
    /// A <see cref="DataParameterGeneric{T}"/> that manages a 64-bit double precision floating point
    /// number (aka, a double). This also has an optional minimum and maximum value range
    /// </summary>
    public sealed class DataParameterDouble : DataParameterGeneric<double> {
        /// <summary>
        /// The minimum value of the parameter. The final effective value may not drop below this
        /// </summary>
        public double Minimum { get; }

        /// <summary>
        /// The maximum value of the parameter. The final effective value may not exceed this
        /// </summary>
        public double Maximum { get; }

        private readonly bool hasRangeLimit;

        public DataParameterDouble(Type ownerType, string key, ValueAccessor<double> accessor, DataParameterFlags flags) : this(ownerType, key, 0.0, accessor, flags) {

        }

        public DataParameterDouble(Type ownerType, string key, double defValue, ValueAccessor<double> accessor, DataParameterFlags flags) : this(ownerType, key, defValue, double.MinValue, double.MaxValue, accessor, flags) {

        }

        public DataParameterDouble(Type ownerType, string key, double defValue, double minValue, double maxValue, ValueAccessor<double> accessor, DataParameterFlags flags) : base(ownerType, key, defValue, accessor, flags) {
            if (minValue > maxValue)
                throw new ArgumentException($"Minimum value exceeds the maximum value: {minValue} > {maxValue}", nameof(minValue));
            if (defValue < minValue || defValue > maxValue)
                throw new ArgumentOutOfRangeException(nameof(defValue), $"Default value ({defValue}) falls out of range of the min/max values ({minValue}/{maxValue})");
            this.Minimum = minValue;
            this.Maximum = maxValue;
            this.hasRangeLimit = minValue > double.MinValue || maxValue < double.MaxValue;
        }

        public double Clamp(double value) => Maths.Clamp(value, this.Minimum, this.Maximum);

        public bool IsValueOutOfRange(double value) => value < this.Minimum || value > this.Maximum;

        public override void SetValue(ITransferableData owner, double value) {
            if (this.hasRangeLimit) {
                value = Maths.Clamp(value, this.Minimum, this.Maximum);
            }

            base.SetValue(owner, value);
        }

        public override void SetObjectValue(ITransferableData owner, object value) {
            if (this.hasRangeLimit) {
                double unboxed = (double) value;
                double clamped = Maths.Clamp(unboxed, this.Minimum, this.Maximum);
                if (!DoubleUtils.AreClose(unboxed, clamped))
                    value = clamped;
            }

            base.SetObjectValue(owner, value);
        }
    }
}