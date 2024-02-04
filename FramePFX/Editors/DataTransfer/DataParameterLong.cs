using System;
using FramePFX.Editors.Automation.Params;
using FramePFX.Utils;

namespace FramePFX.Editors.DataTransfer {
    public sealed class DataParameterLong : DataParameterGeneric<long> {
        /// <summary>
        /// The minimum value of the parameter. The final effective value may not drop below this
        /// </summary>
        public long Minimum { get; }

        /// <summary>
        /// The maximum value of the parameter. The final effective value may not exceed this
        /// </summary>
        public long Maximum { get; }

        private readonly bool hasRangeLimit;

        public DataParameterLong(Type ownerType, string key, ValueAccessor<long> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, key, 0L, accessor, flags) {

        }

        public DataParameterLong(Type ownerType, string key, long defValue, ValueAccessor<long> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, key, defValue, long.MinValue, long.MaxValue, accessor, flags) {

        }

        public DataParameterLong(Type ownerType, string key, long defValue, long minValue, long maxValue, ValueAccessor<long> accessor, DataParameterFlags flags = DataParameterFlags.None) : base(ownerType, key, defValue, accessor, flags) {
            if (minValue > maxValue)
                throw new ArgumentException($"Minimum value exceeds the maximum value: {minValue} > {maxValue}", nameof(minValue));
            if (defValue < minValue || defValue > maxValue)
                throw new ArgumentOutOfRangeException(nameof(defValue), $"Default value ({defValue}) falls out of range of the min/max values ({minValue}/{maxValue})");
            this.Minimum = minValue;
            this.Maximum = maxValue;
            this.hasRangeLimit = minValue != long.MinValue || maxValue != long.MaxValue;
        }

        public double Clamp(double value) => Maths.Clamp(value, this.Minimum, this.Maximum);

        public bool IsValueOutOfRange(double value) => value < this.Minimum || value > this.Maximum;

        public override void SetValue(ITransferableData owner, long value) {
            if (this.hasRangeLimit) {
                value = Maths.Clamp(value, this.Minimum, this.Maximum);
            }

            base.SetValue(owner, value);
        }

        public override void SetObjectValue(ITransferableData owner, object value) {
            if (this.hasRangeLimit) {
                long unboxed = (long) value;
                long clamped = Maths.Clamp(unboxed, this.Minimum, this.Maximum);
                if (unboxed != clamped) {
                    value = clamped;
                }
            }

            base.SetObjectValue(owner, value);
        }
    }
}