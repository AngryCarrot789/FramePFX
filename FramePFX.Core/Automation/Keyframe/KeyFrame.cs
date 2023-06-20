using System;
using System.Numerics;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Automation.Keyframe {
    public abstract class KeyFrame : IRBESerialisable {
        public long Timestamp { get; set; }

        public AutomationSequence OwnerSequence { get; set; }

        public abstract AutomationDataType DataType { get; }

        protected KeyFrame() {

        }

        public virtual void SetDoubleValue(double value) {
            throw new InvalidOperationException();
        }

        public virtual void SetLongValue(long value) {
            throw new InvalidOperationException();
        }

        public virtual void SetBooleanValue(bool value) {
            throw new InvalidOperationException();
        }

        public virtual void SetVector2Value(Vector2 value) {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Returns a blend 'multiplier' between the current instance's timestamp, and the given timestamp, used to
        /// lerp between the current instance's value and the given nextFrame instance's value
        /// <para>
        /// When the return value is less than 0.5, the value is closer towards the current instance's value. Whereas if the value is greater than 0.5, is it closer towards the given nextFrame's value
        /// </para>
        /// </summary>
        /// <param name="nextFrame">Target frame. Its timestamp must be greater than or equal to the current instance's timestamp!</param>
        /// <returns>A blend multiplier</returns>
        public double GetInterpolationMultiplier(long time, KeyFrame nextFrame) {
            long range = nextFrame.Timestamp - this.Timestamp;
            #if DEBUG
            if (range < 0) {
                throw new ArgumentOutOfRangeException(nameof(nextFrame), "Frame must be less than the current instance's value");
            }
            #endif

            if (range == 0) { // exact same timestamp
                return 1d;
            }

            // TODO: implement more than linear interpolation
            // time = 140, this = 100, nextFrame = 200
            // returns 0.4 ((140 - 100) == 40) / (200 - 100)
            return (time - this.Timestamp) / (double) range;
        }

        protected virtual bool Equals(KeyFrame other) {
            return this.Timestamp == other.Timestamp;
        }

        public override bool Equals(object obj) {
            return ReferenceEquals(this, obj) || obj is KeyFrame && this.Equals((KeyFrame) obj);
        }

        public override int GetHashCode() {
            return this.Timestamp.GetHashCode();
        }

        /// <summary>
        /// Writes this key frame data to the given <see cref="RBEDictionary"/>
        /// </summary>
        /// <param name="data"></param>
        public virtual void WriteToRBE(RBEDictionary data) {
            data.SetLong(nameof(this.Timestamp), this.Timestamp);
        }

        /// <summary>
        /// Reads the key frame data from the given <see cref="RBEDictionary"/>
        /// </summary>
        /// <param name="data"></param>
        public virtual void ReadFromRBE(RBEDictionary data) {
            this.Timestamp = data.GetLong(nameof(this.Timestamp));
        }
    }

    public class KeyFrameDouble : KeyFrame {
        public double Value { get; set; }

        public override AutomationDataType DataType => AutomationDataType.Double;

        public KeyFrameDouble() {

        }

        public KeyFrameDouble(long timestamp, double value) {
            this.Timestamp = timestamp;
            this.Value = value;
        }

        public override void SetDoubleValue(double value) {
            this.Value = value;
        }

        public double Interpolate(long time, KeyFrameDouble frame) {
            // realistically, this should never be thrown if the function is used correctly... duh
            if (time < this.Timestamp || time > frame.Timestamp) {
                throw new Exception($"Frame out of range: {time} < {this.Timestamp} || {time} > {frame.Timestamp}");
            }

            double blend = this.GetInterpolationMultiplier(time, frame);

            // this.Value = 2d, frame.Value = 7d
            // ret = (blend * (7d - 2d)) + 2d = 4
            return blend * (frame.Value - this.Value) + this.Value;

            // also see Maths.Interpolate(a, b, blend)
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetDouble(nameof(this.Value), this.Value);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Value = data.GetDouble(nameof(this.Value));
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ this.Value.GetHashCode();
        }

        protected override bool Equals(KeyFrame other) {
            return other is KeyFrameLong keyFrame && base.Equals(keyFrame) && Maths.Equals(keyFrame.Value, this.Value);
        }
    }

    public class KeyFrameLong : KeyFrame {
        public long Value { get; set; }

        /// <summary>
        /// The rounding mode for the interpolation function. See <see cref="Maths.Lerp(long, long, double, int)"/> for more info. Default value = 3
        /// </summary>
        public int RoundingMode { get; set; } = 3;

        public override AutomationDataType DataType => AutomationDataType.Long;

        public KeyFrameLong() {

        }

        public KeyFrameLong(long timestamp, long value) {
            this.Timestamp = timestamp;
            this.Value = value;
        }

        public override void SetLongValue(long value) {
            this.Value = value;
        }

        public long Interpolate(long time, KeyFrameLong frame) {
            // realistically, this should never be thrown if the function is used correctly... duh
            if (time < this.Timestamp || time > frame.Timestamp) {
                throw new Exception($"Frame out of range: {time} < {this.Timestamp} || {time} > {frame.Timestamp}");
            }

            double blend = this.GetInterpolationMultiplier(time, frame);

            // this.Value = 2d, frame.Value = 7d
            // ret = (blend * (7d - 2d)) + 2d = 4
            return Maths.Lerp(this.Value, frame.Value, blend, this.RoundingMode);
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetLong(nameof(this.Value), this.Value);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Value = data.GetLong(nameof(this.Value));
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ this.Value.GetHashCode();
        }

        protected override bool Equals(KeyFrame other) {
            return other is KeyFrameLong keyFrame && base.Equals(keyFrame) && keyFrame.Value == this.Value;
        }
    }

    public class KeyFrameBoolean : KeyFrame {
        public bool Value { get; set; }

        public override AutomationDataType DataType => AutomationDataType.Boolean;

        public KeyFrameBoolean() {

        }

        public KeyFrameBoolean(long timestamp, bool value) {
            this.Timestamp = timestamp;
            this.Value = value;
        }

        public override void SetBooleanValue(bool value) {
            this.Value = value;
        }

        public bool Interpolate(long time, KeyFrameBoolean frame) {
            // realistically, this should never be thrown if the function is used correctly... duh
            if (time < this.Timestamp || time > frame.Timestamp) {
                throw new Exception($"Frame out of range: {time} < {this.Timestamp} || {time} > {frame.Timestamp}");
            }

            bool thisVal = this.Value;
            if (thisVal == frame.Value) {
                return this.Value;
            }

            double blend = this.GetInterpolationMultiplier(time, frame);
            if (blend >= 0.5d) {
                return !thisVal;
            }
            else {
                return thisVal;
            }
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetBool(nameof(this.Value), this.Value);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Value = data.GetBool(nameof(this.Value));
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ this.Value.GetHashCode();
        }

        protected override bool Equals(KeyFrame other) {
            return other is KeyFrameBoolean keyFrame && base.Equals(keyFrame) && keyFrame.Value == this.Value;
        }
    }

    public class KeyFrameVector2 : KeyFrame {
        public Vector2 Value { get; set; }

        public override AutomationDataType DataType => AutomationDataType.Vector2;

        public KeyFrameVector2() {

        }

        public KeyFrameVector2(long timestamp, Vector2 value) {
            this.Timestamp = timestamp;
            this.Value = value;
        }

        public override void SetVector2Value(Vector2 value) {
            this.Value = value;
        }

        public Vector2 Interpolate(long time, KeyFrameVector2 frame) {
            // realistically, this should never be thrown if the function is used correctly... duh
            if (time < this.Timestamp || time > frame.Timestamp) {
                throw new Exception($"Frame out of range: {time} < {this.Timestamp} || {time} > {frame.Timestamp}");
            }

            double blend = this.GetInterpolationMultiplier(time, frame);
            return this.Value.Lerp(frame.Value, blend);
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetStruct(nameof(this.Value), this.Value);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Value = data.GetStruct<Vector2>(nameof(this.Value));
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ this.Value.GetHashCode();
        }

        protected override bool Equals(KeyFrame other) {
            return other is KeyFrameVector2 keyFrame && base.Equals(keyFrame) && keyFrame.Value == this.Value;
        }
    }
}