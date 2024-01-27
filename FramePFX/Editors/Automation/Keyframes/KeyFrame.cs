using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FramePFX.Editors.Automation.Params;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editors.Automation.Keyframes {
    public delegate void KeyFramePositionChangedEventHandler(KeyFrame keyFrame, long oldFrame, long newFrame);
    public delegate void KeyFrameEventHandler(KeyFrame keyFrame);

    public delegate void FloatKeyFrameValueChanged(KeyFrameFloat keyFrame, float oldValue, float newValue);
    public delegate void DoubleKeyFrameValueChanged(KeyFrameDouble keyFrame, double oldValue, double newValue);
    public delegate void LongKeyFrameValueChanged(KeyFrameLong keyFrame, long oldValue, long newValue);
    public delegate void BooleanKeyFrameValueChanged(KeyFrameBoolean keyFrame, bool oldValue, bool newValue);

    /// <summary>
    /// A keyframe stores a time and value
    /// </summary>
    public abstract class KeyFrame {
        protected long myFrame;
        public AutomationSequence sequence;
        public double curveBend = 0D; // -1d to +1d. // A 'bend' in the interpolation. could add something more complicated?

        public long Frame {
            get => this.myFrame;
            set {
                long oldFrame = this.myFrame;
                if (oldFrame == value)
                    return;
                this.myFrame = value;
                this.FrameChanged?.Invoke(this, oldFrame, value);
                AutomationSequence.OnKeyFramePositionChanged(this.sequence, this);
            }
        }

        /// <summary>
        /// This key frame's data type
        /// </summary>
        public abstract AutomationDataType DataType { get; }

        /// <summary>
        /// An event fired when our <see cref="Frame"/> changes
        /// </summary>
        public event KeyFramePositionChangedEventHandler FrameChanged;

        /// <summary>
        /// An event fired when this key frame's value changes. Because this event is in
        /// the base class (fired by the derived classes), the previous value cannot be accessed
        /// </summary>
        public event KeyFrameEventHandler ValueChanged;

        protected KeyFrame() {
        }

        protected virtual void OnValueChanged() {
            this.ValueChanged?.Invoke(this);
        }

        /// <summary>
        /// Unboxes (if primitive) the value and sets it as this key frame's value
        /// </summary>
        /// <param name="value">The new value</param>
        public abstract void SetValueFromObject(object value);

        /// <summary>
        /// Boxes this key frame's value
        /// </summary>
        /// <returns>The possibly boxed value</returns>
        public abstract object GetObjectValue();

        internal static void SetupDefaultKeyFrameForSequence(KeyFrame frame, AutomationSequence sequence) {
            frame.sequence = sequence;
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

        #endregion

        /// <summary>
        /// Assigns this key frame's value to the given key descriptor's default value
        /// </summary>
        public abstract void AssignDefaultValue(ParameterDescriptor desc);

        /// <summary>
        /// Assigns this key frame's value to what the given sequence evaluates at the given frame
        /// </summary>
        public abstract void AssignCurrentValue(long frame, AutomationSequence seq, bool ignoreOverrideState = false);

        /// <summary>
        /// Whether or not the given key frame equals this key frame (equal timestamp and value)
        /// </summary>
        /// <param name="other">The other value to compare</param>
        /// <returns>True when this instance and the other instance are effectively equal (matching timestamp and value)</returns>
        public virtual bool IsEqualTo(KeyFrame other) {
            return this.myFrame == other.myFrame;
        }

        /// <summary>
        /// Writes this key frame data to the given <see cref="RBEDictionary"/>
        /// </summary>
        /// <param name="data"></param>
        public virtual void WriteToRBE(RBEDictionary data) {
            data.SetULong("Time", (ulong) this.myFrame);
        }

        /// <summary>
        /// Reads the key frame data from the given <see cref="RBEDictionary"/>
        /// </summary>
        /// <param name="data"></param>
        public virtual void ReadFromRBE(RBEDictionary data) {
            this.myFrame = (long) data.GetULong("Time");
        }

        #region Factory Methods

        /// <summary>
        /// Factory method for creating an instance of a key frame from the enum data type
        /// </summary>
        /// <param name="type">Type of key frame to create</param>
        /// <returns>A new key frame instance</returns>
        /// <exception cref="ArgumentOutOfRangeException">Unknown automation data type</exception>
        [SwitchAutomationDataType]
        public static KeyFrame CreateInstance(AutomationDataType type) {
            switch (type) {
                case AutomationDataType.Float:   return new KeyFrameFloat();
                case AutomationDataType.Double:  return new KeyFrameDouble();
                case AutomationDataType.Long:    return new KeyFrameLong();
                case AutomationDataType.Boolean: return new KeyFrameBoolean();
                default: throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid automation data type enum");
            }
        }

        /// <summary>
        /// Creates a new instance of the given automation data type, and sets the key frame's value to the actual value at a specific frame
        /// </summary>
        /// <param name="type">Type of key frame to create</param>
        /// <param name="time">The frame at which the returned key frame will be at</param>
        /// <param name="sequence">The sequence, in order to access to actual value at the given frame</param>
        /// <returns>A new key frame instance</returns>
        /// <exception cref="ArgumentOutOfRangeException">Unknown automation data type</exception>
        public static KeyFrame CreateInstance(AutomationSequence sequence, long time = 0L) {
            KeyFrame keyFrame = CreateInstance(sequence.DataType); // same as sequence.Key.CreateKeyFrame()
            keyFrame.myFrame = time;
            keyFrame.AssignCurrentValue(time, sequence);
            return keyFrame;
        }

        /// <summary>
        /// Creates an instance of a key frame which is suitable for the given key, and assigns its default value
        /// </summary>
        public static KeyFrame CreateDefault(Parameter parameter, long time = 0L) {
            KeyFrame keyFrame = CreateInstance(parameter.DataType);
            keyFrame.myFrame = time;
            keyFrame.AssignDefaultValue(parameter.Descriptor);
            return keyFrame;
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

            if (range == 0) {
                // exact same timestamp
                return 1d;
            }

            double blend = (time - timeA) / (double) range;

            // not used at the moment; doesn't seem to work property and I can't
            // get it to render/behave correctly in the UI when rendered
            if (curve != 0d) {
                blend = Math.Pow(blend, 1d / Math.Abs(curve));
                // if (curve < 0d) {
                //     blend = 1d - blend;
                // }
            }

            return blend;
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
        /// Returns a blend 'multiplier' between the current instance's timestamp, and the given timestamp,
        /// used to lerp between the current instance's value and the given nextFrame instance's value
        /// <para>
        /// When the return value is less than 0.5, the value is closer towards the current instance's value. Whereas if the value is greater than 0.5, is it closer towards the given nextFrame's value
        /// </para>
        /// </summary>
        /// <param name="targetTime">Target timestamp. Must be greater than or equal to the current instance's timestamp, otherwise undefined behaviour may occur</param>
        /// <returns>A blend multiplier</returns>
        public double GetInterpolationMultiplier(long frame, long targetTime) {
            return GetInterpolationLerp(frame, this.myFrame, targetTime, this.curveBend);
        }

        /// <summary>
        /// <inheritdoc cref="GetInterpolationMultiplier(long,long)"/>
        /// </summary>
        /// <param name="frame">The timestamp, which is between the current instance's timestamp, and <see cref="nextFrame"/>'s timestamp</param>
        /// <param name="nextFrame">Target frame. Its timestamp must be greater than or equal to the current instance's timestamp, otherwise undefined behaviour may occur</param>
        /// <returns>A blend multiplier</returns>
        public double GetInterpolationMultiplier(long frame, KeyFrame nextFrame) {
            return this.GetInterpolationMultiplier(frame, nextFrame.myFrame);
        }

        #endregion

        #region Other helpers

        [Conditional("DEBUG")]
        protected void ValidateTime(long t, KeyFrame frame) {
            // realistically, this should never be thrown if the function is used correctly... duh
            if (t < this.myFrame || t > frame.myFrame) {
                throw new Exception($"Time out of range: {t} < {this.myFrame} || {t} > {frame.myFrame}");
            }
        }

        #endregion
    }

    public class KeyFrameFloat : KeyFrame {
        private float myValue;

        public float Value {
            get => this.myValue;
            set {
                float oldValue = this.myValue;
                if (oldValue == value)
                    return;
                this.myValue = value;
                this.OnValueChanged();
                this.FloatValueChanged?.Invoke(this, oldValue, value);
                AutomationSequence.OnKeyFrameValueChanged(this.sequence, this);
            }
        }

        public event FloatKeyFrameValueChanged FloatValueChanged;

        public override AutomationDataType DataType => AutomationDataType.Float;

        public KeyFrameFloat() { }

        public KeyFrameFloat(long frame, float value) {
            this.myFrame = frame;
            this.Value = value;
        }

        public override void SetValueFromObject(object value) => this.Value = (float) value;

        public override object GetObjectValue() => this.Value;

        public override void AssignDefaultValue(ParameterDescriptor desc) => this.Value = ((ParameterDescriptorFloat) desc).DefaultValue;

        public override void AssignCurrentValue(long frame, AutomationSequence seq, bool ignoreOverrideState = false) => this.Value = seq.GetFloatValue(frame, ignoreOverrideState);

        public float Interpolate(long time, KeyFrameFloat frame) {
            this.ValidateTime(time, frame);
            double blend = this.GetInterpolationMultiplier(time, frame.myFrame);
            return (float) (blend * (frame.myValue - this.myValue)) + this.myValue;
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetFloat(nameof(this.Value), this.myValue);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.myValue = data.GetFloat(nameof(this.Value));
        }

        public override bool IsEqualTo(KeyFrame other) {
            return base.IsEqualTo(other) && other is KeyFrameFloat keyFrame && Maths.Equals(keyFrame.myValue, this.myValue);
        }
    }

    public class KeyFrameDouble : KeyFrame {
        private double myValue;

        public double Value {
            get => this.myValue;
            set {
                double oldValue = this.myValue;
                if (oldValue == value)
                    return;
                this.myValue = value;
                this.OnValueChanged();
                this.DoubleValueChanged?.Invoke(this, oldValue, value);
                AutomationSequence.OnKeyFrameValueChanged(this.sequence, this);
            }
        }

        public event DoubleKeyFrameValueChanged DoubleValueChanged;

        public override AutomationDataType DataType => AutomationDataType.Double;

        public KeyFrameDouble() { }

        public KeyFrameDouble(long frame, double value) {
            this.myFrame = frame;
            this.Value = value;
        }

        public override void SetValueFromObject(object value) => this.Value = (double) value;

        public override object GetObjectValue() => this.Value;

        public override void AssignDefaultValue(ParameterDescriptor desc) => this.Value = ((ParameterDescriptorDouble) desc).DefaultValue;

        public override void AssignCurrentValue(long frame, AutomationSequence seq, bool ignoreOverrideState = false) => this.Value = seq.GetDoubleValue(frame, ignoreOverrideState);

        public double Interpolate(long time, KeyFrameDouble nextFrame) {
            this.ValidateTime(time, nextFrame);
            double blend = this.GetInterpolationMultiplier(time, nextFrame.myFrame);
            return blend * (nextFrame.myValue - this.myValue) + this.myValue;
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetDouble(nameof(this.Value), this.myValue);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.myValue = data.GetDouble(nameof(this.Value));
        }

        public override bool IsEqualTo(KeyFrame other) {
            return base.IsEqualTo(other) && other is KeyFrameDouble keyFrame && Maths.Equals(keyFrame.myValue, this.myValue);
        }
    }

    public class KeyFrameLong : KeyFrame {
        private long myValue;

        public long Value {
            get => this.myValue;
            set {
                long oldValue = this.myValue;
                if (oldValue == value)
                    return;
                this.myValue = value;
                this.OnValueChanged();
                this.LongValueChanged?.Invoke(this, oldValue, value);
                AutomationSequence.OnKeyFrameValueChanged(this.sequence, this);
            }
        }

        public event LongKeyFrameValueChanged LongValueChanged;

        /// <summary>
        /// The rounding mode for the interpolation function. See <see cref="Maths.Lerp(long, long, double, int)"/> for more info. Default value = 3
        /// </summary>
        public int RoundingMode = 3;

        public override AutomationDataType DataType => AutomationDataType.Long;

        public KeyFrameLong() { }

        public KeyFrameLong(long frame, long value) {
            this.myFrame = frame;
            this.Value = value;
        }

        public override void SetValueFromObject(object value) => this.Value = (long) value;

        public override object GetObjectValue() => this.Value;

        public override void AssignDefaultValue(ParameterDescriptor desc) => this.Value = ((ParameterDescriptorLong) desc).DefaultValue;

        public override void AssignCurrentValue(long frame, AutomationSequence seq, bool ignoreOverrideState = false) => this.Value = seq.GetLongValue(frame, ignoreOverrideState);

        public long Interpolate(long time, KeyFrameLong frame) {
            this.ValidateTime(time, frame);
            double blend = this.GetInterpolationMultiplier(time, frame);
            return Maths.Lerp(this.Value, frame.Value, blend, this.RoundingMode);
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetLong(nameof(this.Value), this.myValue);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.myValue = data.GetLong(nameof(this.Value));
        }

        public override bool IsEqualTo(KeyFrame other) {
            return base.IsEqualTo(other) && other is KeyFrameLong keyFrame && keyFrame.myValue == this.myValue;
        }
    }

    public class KeyFrameBoolean : KeyFrame {
        private bool myValue;

        public bool Value {
            get => this.myValue;
            set {
                bool oldValue = this.myValue;
                if (oldValue == value)
                    return;
                this.myValue = value;
                this.OnValueChanged();
                this.BooleanValueChanged?.Invoke(this, oldValue, value);
                AutomationSequence.OnKeyFrameValueChanged(this.sequence, this);
            }
        }

        public event BooleanKeyFrameValueChanged BooleanValueChanged;

        public override AutomationDataType DataType => AutomationDataType.Boolean;

        public KeyFrameBoolean() { }

        public KeyFrameBoolean(long frame, bool value) {
            this.myFrame = frame;
            this.Value = value;
        }

        public override void SetValueFromObject(object value) => this.Value = (bool) value;

        public override object GetObjectValue() => this.Value.Box();

        public override void AssignDefaultValue(ParameterDescriptor desc) => this.Value = ((ParameterDescriptorBoolean) desc).DefaultValue;

        public override void AssignCurrentValue(long frame, AutomationSequence seq, bool ignoreOverrideState = false) => this.Value = seq.GetBooleanValue(frame, ignoreOverrideState);

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
            data.SetBool(nameof(this.Value), this.myValue);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.myValue = data.GetBool(nameof(this.Value));
        }

        public override bool IsEqualTo(KeyFrame other) {
            return base.IsEqualTo(other) && other is KeyFrameBoolean keyFrame && keyFrame.myValue == this.myValue;
        }
    }
}