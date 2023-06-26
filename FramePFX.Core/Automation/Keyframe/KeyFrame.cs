using System;
using System.Numerics;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Automation.Keyframe {
    public abstract class KeyFrame : IRBESerialisable {
        public AutomationSequence OwnerSequence;
        public long Timestamp;
        public double CurveBendAmount = 0D; // -1d to +1d

        public abstract AutomationDataType DataType { get; }

        protected KeyFrame() {

        }

        public void SetFloatValue(float value) => ((KeyFrameFloat) this).Value = value;
        public void SetDoubleValue(double value) => ((KeyFrameDouble) this).Value = value;
        public void SetLongValue(long value) => ((KeyFrameLong) this).Value = value;
        public void SetBooleanValue(bool value) => ((KeyFrameBoolean) this).Value = value;
        public void SetVector2Value(Vector2 value) => ((KeyFrameVector2) this).Value = value;

        public static double GetInterpolationMultiplier(long time, long timeA, long timeB, double curve) {
            long range = timeB - timeA;
            #if DEBUG
            if (range < 0) {
                throw new ArgumentOutOfRangeException(nameof(timeB), "Next time must be less than the current instance's value");
            }
            #endif

            if (range == 0) { // exact same timestamp
                return 1d;
            }

            double blend = (time - timeA) / (double) range;
            if (curve != 0d) {
                blend = Math.Pow(blend, 1d / Math.Abs(curve));
                if (curve < 0d) {
                    blend = 1d - blend;
                }
            }

            return blend;
        }

        public double GetInterpolationMultiplier(long time, long targetTime) {
            return GetInterpolationMultiplier(time, this.Timestamp, targetTime, this.CurveBendAmount);
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
            return this.GetInterpolationMultiplier(time, nextFrame.Timestamp);
        }

        // demo interpolation
        // public static float GetMultiplier(long time, long timeA, long timeB) {
        //     long range = timeB - timeA; // assert range >= 0
        //     if (range == 0) // exact same timestamp
        //         return 1f;
        //     return (time - timeA) / (float) range;
        // }
        // public static float Interpolate(long time, long timeA, long timeB, float valA, float valB) {
        //     float blend = GetMultiplier(time, timeA, timeB);
        //     return blend * (valB - valA) + valA;
        // }

        /// <summary>
        /// Whether or not the given key frame equals this key frame (equal timestamp and value)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool Equals(KeyFrame other) {
            return this.Timestamp == other.Timestamp;
        }

        /// <summary>
        /// Whether or not the given value equals this key frame
        /// </summary>
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

    public class KeyFrameFloat : KeyFrame {
        public float Value;

        public override AutomationDataType DataType => AutomationDataType.Float;

        public KeyFrameFloat() {

        }

        public KeyFrameFloat(long timestamp, float value) {
            this.Timestamp = timestamp;
            this.Value = value;
        }

        public float Interpolate(long time, KeyFrameFloat frame) {
            // realistically, this should never be thrown if the function is used correctly... duh
            if (time < this.Timestamp || time > frame.Timestamp) {
                throw new Exception($"Frame out of range: {time} < {this.Timestamp} || {time} > {frame.Timestamp}");
            }

            double blend = this.GetInterpolationMultiplier(time, frame);

            // this.Value = 2d, frame.Value = 7d
            // ret = (blend * (7d - 2d)) + 2d = 4
            return (float) (blend * (frame.Value - this.Value)) + this.Value;
            // also see Maths.Interpolate(a, b, blend)
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetFloat(nameof(this.Value), this.Value);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Value = data.GetFloat(nameof(this.Value));
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ this.Value.GetHashCode();
        }

        public override bool Equals(KeyFrame other) {
            return base.Equals(other) && other is KeyFrameFloat keyFrame && Maths.Equals(keyFrame.Value, this.Value);
        }
    }

    public class KeyFrameDouble : KeyFrame {
        public double Value;

        public override AutomationDataType DataType => AutomationDataType.Double;

        public KeyFrameDouble() {

        }

        public KeyFrameDouble(long timestamp, double value) {
            this.Timestamp = timestamp;
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

        public override bool Equals(KeyFrame other) {
            return base.Equals(other) && other is KeyFrameDouble keyFrame && Maths.Equals(keyFrame.Value, this.Value);
        }
    }

    public class KeyFrameLong : KeyFrame {
        public long Value;

        /// <summary>
        /// The rounding mode for the interpolation function. See <see cref="Maths.Lerp(long, long, double, int)"/> for more info. Default value = 3
        /// </summary>
        public int RoundingMode = 3;

        public override AutomationDataType DataType => AutomationDataType.Long;

        public KeyFrameLong() {

        }

        public KeyFrameLong(long timestamp, long value) {
            this.Timestamp = timestamp;
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

        public override bool Equals(KeyFrame other) {
            return base.Equals(other) && other is KeyFrameLong keyFrame && keyFrame.Value == this.Value;
        }
    }

    public class KeyFrameBoolean : KeyFrame {
        public bool Value;

        public override AutomationDataType DataType => AutomationDataType.Boolean;

        public KeyFrameBoolean() {

        }

        public KeyFrameBoolean(long timestamp, bool value) {
            this.Timestamp = timestamp;
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

        public override bool Equals(KeyFrame other) {
            return base.Equals(other) && other is KeyFrameBoolean keyFrame && keyFrame.Value == this.Value;
        }
    }

    public class KeyFrameVector2 : KeyFrame {
        public Vector2 Value;

        public override AutomationDataType DataType => AutomationDataType.Vector2;

        public KeyFrameVector2() {

        }

        public KeyFrameVector2(long timestamp, Vector2 value) {
            this.Timestamp = timestamp;
            this.Value = value;
        }

        public Vector2 Interpolate(long time, KeyFrameVector2 frame) {
            // realistically, this should never be thrown if the function is used correctly... duh
            if (time < this.Timestamp || time > frame.Timestamp) {
                throw new Exception($"Frame out of range: {time} < {this.Timestamp} || {time} > {frame.Timestamp}");
            }

            double blend = this.GetInterpolationMultiplier(time, frame);
            return this.Value.Lerp(frame.Value, (float) blend);
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

        public override bool Equals(KeyFrame other) {
            return base.Equals(other) && other is KeyFrameVector2 keyFrame && keyFrame.Value == this.Value;
        }
    }
}