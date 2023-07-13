using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Editor;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Automation.Keyframe {
    public abstract class KeyFrame : IRBESerialisable {
        // TODO: move from frame time to frame-independent time, using this as a timebase
        // targetTime *

        public static Rational DELTA = new Rational(1, 1000);

        public AutomationSequence sequence;
        public long time;
        public double curveBend = 0D; // -1d to +1d

        /// <summary>
        /// This key frame's data type
        /// </summary>
        public abstract AutomationDataType DataType { get; }

        protected KeyFrame() {

        }

        #region Getter functions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetFloatValue() => ((KeyFrameFloat) this).Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetDoubleValue() => ((KeyFrameDouble) this).Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetLongValue() => ((KeyFrameLong) this).Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetBooleanValue() => ((KeyFrameBoolean) this).Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GetVector2Value() => ((KeyFrameVector2) this).Value;

        #endregion

        #region Setter functions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFloatValue(float value) => ((KeyFrameFloat) this).Value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetDoubleValue(double value) => ((KeyFrameDouble) this).Value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLongValue(long value) => ((KeyFrameLong) this).Value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBooleanValue(bool value) => ((KeyFrameBoolean) this).Value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVector2Value(Vector2 value) => ((KeyFrameVector2) this).Value = value;

        #endregion

        public abstract void AssignDefaultValue(KeyDescriptor desc);

        public abstract void AssignCurrentValue(long frame, AutomationSequence seq);

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
        /// <param name="other">The other value to compare</param>
        /// <returns>True when this instance and the other instance are effectively equal (matching timestamp and value)</returns>
        public virtual bool IsEqualTo(KeyFrame other) {
            return this.time == other.time;
        }

        /// <summary>
        /// Writes this key frame data to the given <see cref="RBEDictionary"/>
        /// </summary>
        /// <param name="data"></param>
        public virtual void WriteToRBE(RBEDictionary data) {
            data.SetULong(nameof(this.time), (ulong) this.time);
        }

        /// <summary>
        /// Reads the key frame data from the given <see cref="RBEDictionary"/>
        /// </summary>
        /// <param name="data"></param>
        public virtual void ReadFromRBE(RBEDictionary data) {
            this.time = (long) data.GetULong(nameof(this.time));
        }

        #region Factory Methods

        /// <summary>
        /// Factory method for creating an instance of a key frame from the enum data type
        /// </summary>
        /// <param name="type">Type of key frame to create</param>
        /// <returns>A new key frame instance</returns>
        /// <exception cref="ArgumentOutOfRangeException">Unknown automation data type</exception>
        public static KeyFrame CreateInstance(AutomationDataType type) {
            switch (type) {
                case AutomationDataType.Float:   return new KeyFrameFloat();
                case AutomationDataType.Double:  return new KeyFrameDouble();
                case AutomationDataType.Long:    return new KeyFrameLong();
                case AutomationDataType.Boolean: return new KeyFrameBoolean();
                case AutomationDataType.Vector2: return new KeyFrameVector2();
                default: throw new ArgumentOutOfRangeException(nameof(type), $"Invalid data type: {type}");
            }
        }

        /// <summary>
        /// Creates a new instance of the given automation data type, and sets the key frame's value to the actual value at a specific frame
        /// </summary>
        /// <param name="type">Type of key frame to create</param>
        /// <param name="frame">The frame at which the returned key frame will be at</param>
        /// <param name="sequence">The sequence, in order to access to actual value at the given frame</param>
        /// <returns>A new key frame instance</returns>
        /// <exception cref="ArgumentOutOfRangeException">Unknown automation data type</exception>
        public static KeyFrame CreateInstance(AutomationDataType type, long frame, AutomationSequence sequence) {
            switch (type) {
                case AutomationDataType.Float:   return new KeyFrameFloat(frame, sequence.GetFloatValue(frame));
                case AutomationDataType.Double:  return new KeyFrameDouble(frame, sequence.GetDoubleValue(frame));
                case AutomationDataType.Long:    return new KeyFrameLong(frame, sequence.GetLongValue(frame));
                case AutomationDataType.Boolean: return new KeyFrameBoolean(frame, sequence.GetBooleanValue(frame));
                case AutomationDataType.Vector2: return new KeyFrameVector2(frame, sequence.GetVector2Value(frame));
                default: throw new ArgumentOutOfRangeException(nameof(type), $"Invalid data type: {type}");
            }
        }

        #endregion

        #region Interpolation helper functions

        /// <summary>
        /// A helper function for generating a lerp multiplier for a given time (between the range timeA and timeB),
        /// while also calculating an interpolation curve multiplier (to bend the line)
        /// </summary>
        /// <param name="time">Input time (normally between <see cref="timeA"/> and <see cref="timeB"/>)</param>
        /// <param name="timeA">Start time</param>
        /// <param name="timeB">End time</param>
        /// <param name="curve">A curve value which affects the output value. 0d by default, meaning a direct lerp</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static double GetInterpolationLerp(long time, long timeA, long timeB, double curve) {
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

        /// <summary>
        /// Returns a blend 'multiplier' between the current instance's timestamp, and the given timestamp,
        /// used to lerp between the current instance's value and the given nextFrame instance's value
        /// <para>
        /// When the return value is less than 0.5, the value is closer towards the current instance's value. Whereas if the value is greater than 0.5, is it closer towards the given nextFrame's value
        /// </para>
        /// </summary>
        /// <param name="targetTime">Target timestamp. Must be greater than or equal to the current instance's timestamp, otherwise undefined behaviour may occur</param>
        /// <returns>A blend multiplier</returns>
        public double GetInterpolationMultiplier(long timestamp, long targetTime) {
            return GetInterpolationLerp(timestamp, this.time, targetTime, this.curveBend);
        }

        /// <summary>
        /// <inheritdoc cref="GetInterpolationMultiplier(long,long)"/>
        /// </summary>
        /// <param name="timestamp">The timestamp, which is between the current instance's timestamp, and <see cref="nextFrame"/>'s timestamp</param>
        /// <param name="nextFrame">Target frame. Its timestamp must be greater than or equal to the current instance's timestamp, otherwise undefined behaviour may occur</param>
        /// <returns>A blend multiplier</returns>
        public double GetInterpolationMultiplier(long timestamp, KeyFrame nextFrame) {
            return this.GetInterpolationMultiplier(timestamp, nextFrame.time);
        }

        #endregion

        #region Other helpers

        [Conditional("DEBUG")]
        protected void ValidateTime(long t, KeyFrame frame) {
            // realistically, this should never be thrown if the function is used correctly... duh
            if (t < this.time || t > frame.time) {
                throw new Exception($"Time out of range: {t} < {this.time} || {t} > {frame.time}");
            }
        }

        #endregion
    }

    public class KeyFrameFloat : KeyFrame {
        public float Value;

        public override AutomationDataType DataType => AutomationDataType.Float;

        public KeyFrameFloat() { }

        public KeyFrameFloat(long time, float value) {
            this.time = time;
            this.Value = value;
        }

        public override void AssignDefaultValue(KeyDescriptor desc) => this.Value = ((KeyDescriptorFloat) desc).DefaultValue;

        public override void AssignCurrentValue(long frame, AutomationSequence seq) => this.Value = seq.GetFloatValue(frame, true);

        public float Interpolate(long time, KeyFrameFloat frame) {
            this.ValidateTime(time, frame);
            double blend = this.GetInterpolationMultiplier(time, frame.time);
            return (float) (blend * (frame.Value - this.Value)) + this.Value;
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetFloat(nameof(this.Value), this.Value);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Value = data.GetFloat(nameof(this.Value));
        }

        public override bool IsEqualTo(KeyFrame other) {
            return base.IsEqualTo(other) && other is KeyFrameFloat keyFrame && Maths.Equals(keyFrame.Value, this.Value);
        }
    }

    public class KeyFrameDouble : KeyFrame {
        public double Value;

        public override AutomationDataType DataType => AutomationDataType.Double;

        public KeyFrameDouble() { }

        public KeyFrameDouble(long time, double value) {
            this.time = time;
            this.Value = value;
        }

        public override void AssignDefaultValue(KeyDescriptor desc) => this.Value = ((KeyDescriptorDouble) desc).DefaultValue;

        public override void AssignCurrentValue(long frame, AutomationSequence seq) => this.Value = seq.GetDoubleValue(frame, true);

        public double Interpolate(long time, KeyFrameDouble nextFrame) {
            this.ValidateTime(time, nextFrame);
            double blend = this.GetInterpolationMultiplier(time, nextFrame.time);
            return blend * (nextFrame.Value - this.Value) + this.Value;
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetDouble(nameof(this.Value), this.Value);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Value = data.GetDouble(nameof(this.Value));
        }

        public override bool IsEqualTo(KeyFrame other) {
            return base.IsEqualTo(other) && other is KeyFrameDouble keyFrame && Maths.Equals(keyFrame.Value, this.Value);
        }
    }

    public class KeyFrameLong : KeyFrame {
        public long Value;

        /// <summary>
        /// The rounding mode for the interpolation function. See <see cref="Maths.Lerp(long, long, double, int)"/> for more info. Default value = 3
        /// </summary>
        public int RoundingMode = 3;

        public override AutomationDataType DataType => AutomationDataType.Long;

        public KeyFrameLong() { }

        public KeyFrameLong(long time, long value) {
            this.time = time;
            this.Value = value;
        }

        public override void AssignDefaultValue(KeyDescriptor desc) => this.Value = ((KeyDescriptorLong) desc).DefaultValue;

        public override void AssignCurrentValue(long frame, AutomationSequence seq) => this.Value = seq.GetLongValue(frame, true);

        public long Interpolate(long time, KeyFrameLong frame) {
            this.ValidateTime(time, frame);
            double blend = this.GetInterpolationMultiplier(time, frame);
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

        public override bool IsEqualTo(KeyFrame other) {
            return base.IsEqualTo(other) && other is KeyFrameLong keyFrame && keyFrame.Value == this.Value;
        }
    }

    public class KeyFrameBoolean : KeyFrame {
        public bool Value;

        public override AutomationDataType DataType => AutomationDataType.Boolean;

        public KeyFrameBoolean() { }

        public KeyFrameBoolean(long time, bool value) {
            this.time = time;
            this.Value = value;
        }

        public override void AssignDefaultValue(KeyDescriptor desc) => this.Value = ((KeyDescriptorBoolean) desc).DefaultValue;

        public override void AssignCurrentValue(long frame, AutomationSequence seq) => this.Value = seq.GetBooleanValue(frame, true);

        public bool Interpolate(long time, KeyFrameBoolean frame) {
            this.ValidateTime(time, frame);
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

        public override bool IsEqualTo(KeyFrame other) {
            return base.IsEqualTo(other) && other is KeyFrameBoolean keyFrame && keyFrame.Value == this.Value;
        }
    }

    public class KeyFrameVector2 : KeyFrame {
        public Vector2 Value;

        public override AutomationDataType DataType => AutomationDataType.Vector2;

        public KeyFrameVector2() { }

        public KeyFrameVector2(long time, Vector2 value) {
            this.time = time;
            this.Value = value;
        }

        public override void AssignDefaultValue(KeyDescriptor desc) => this.Value = ((KeyDescriptorVector2) desc).DefaultValue;

        public override void AssignCurrentValue(long frame, AutomationSequence seq) => this.Value = seq.GetVector2Value(frame, true);

        public Vector2 Interpolate(long time, KeyFrameVector2 frame) {
            this.ValidateTime(time, frame);
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

        public override bool IsEqualTo(KeyFrame other) {
            return base.IsEqualTo(other) && other is KeyFrameVector2 keyFrame && keyFrame.Value == this.Value;
        }
    }
}