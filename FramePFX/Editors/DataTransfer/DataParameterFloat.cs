using System;
using FramePFX.Editors.Automation.Params;
using FramePFX.Utils;

namespace FramePFX.Editors.DataTransfer {
    /// <summary>
    /// A <see cref="DataParameterGeneric{T}"/> that manages a 32-bit single precision floating point
    /// number (aka, a float). This also has an optional minimum and maximum value range
    /// </summary>
    public sealed class DataParameterFloat : DataParameterGeneric<float> {
        /// <summary>
        /// The minimum value of the parameter. The final effective value may not drop below this
        /// </summary>
        public float Minimum { get; }

        /// <summary>
        /// The maximum value of the parameter. The final effective value may not exceed this
        /// </summary>
        public float Maximum { get; }

        private readonly bool hasRangeLimit;

        public DataParameterFloat(Type ownerType, string key, ValueAccessor<float> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, key, 0.0F, accessor, flags) {

        }

        public DataParameterFloat(Type ownerType, string key, float defValue, ValueAccessor<float> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, key, defValue, float.MinValue, float.MaxValue, accessor, flags) {

        }

        public DataParameterFloat(Type ownerType, string key, float defValue, float minValue, float maxValue, ValueAccessor<float> accessor, DataParameterFlags flags = DataParameterFlags.None) : base(ownerType, key, defValue, accessor, flags) {
            if (minValue > maxValue)
                throw new ArgumentException($"Minimum value exceeds the maximum value: {minValue} > {maxValue}", nameof(minValue));
            if (defValue < minValue || defValue > maxValue)
                throw new ArgumentOutOfRangeException(nameof(defValue), $"Default value ({defValue}) falls out of range of the min/max values ({minValue}/{maxValue})");
            this.Minimum = minValue;
            this.Maximum = maxValue;
            this.hasRangeLimit = minValue > float.MinValue || maxValue < float.MaxValue;
        }

        public float Clamp(float value) => Maths.Clamp(value, this.Minimum, this.Maximum);

        public bool IsValueOutOfRange(float value) => value < this.Minimum || value > this.Maximum;

        public override void SetValue(ITransferableData owner, float value) {
            if (this.hasRangeLimit) {
                value = Maths.Clamp(value, this.Minimum, this.Maximum);
            }

            base.SetValue(owner, value);
        }

        public override void SetObjectValue(ITransferableData owner, object value) {
            if (this.hasRangeLimit) {
                float unboxed = (float) value;
                float clamped = Maths.Clamp(unboxed, this.Minimum, this.Maximum);
                if (!Maths.Equals(unboxed, clamped))
                    value = clamped;
            }

            base.SetObjectValue(owner, value);
        }
    }
}