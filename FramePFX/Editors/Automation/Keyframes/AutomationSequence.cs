using System;
using System.Collections.Generic;
using System.Numerics;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Timelines;
using FramePFX.RBC;

namespace FramePFX.Editors.Automation.Keyframes {
    public delegate void KeyFrameAddedEventHandler(AutomationSequence sequence, KeyFrame keyFrame, int index);
    public delegate void KeyFrameRemovedEventHandler(AutomationSequence sequence, KeyFrame keyFrame, int index);
    public delegate void AutomationSequenceEventHandler(AutomationSequence sequence);

    /// <summary>
    /// An automation sequence contains all of the key frames for a specific <see cref="Params.Parameter"/>,
    /// for an automatable object (accessible via our <see cref="AutomationData"/>'s <see cref="Automation.AutomationData.Owner"/> property)
    /// </summary>
    public class AutomationSequence {
        private static readonly Func<KeyFrame, float> FuncGetFloat = k => ((KeyFrameFloat) k).Value;
        private static readonly Func<KeyFrame, double> FuncGetDouble = k => ((KeyFrameDouble) k).Value;
        private static readonly Func<KeyFrame, long> FuncGetLong = k => ((KeyFrameLong) k).Value;
        private static readonly Func<KeyFrame, bool> FuncGetBool = k => ((KeyFrameBoolean) k).Value;
        private static readonly Func<KeyFrame, Vector2> FuncGetVector2 = k => ((KeyFrameVector2) k).Value;
        private static readonly Func<long, KeyFrame, KeyFrame, float> FuncCalcFloat = (t, a, b) => ((KeyFrameFloat) a).Interpolate(t, (KeyFrameFloat) b);
        private static readonly Func<long, KeyFrame, KeyFrame, double> FuncCalcDouble = (t, a, b) => ((KeyFrameDouble) a).Interpolate(t, (KeyFrameDouble) b);
        private static readonly Func<long, KeyFrame, KeyFrame, long> FuncCalcLong = (t, a, b) => ((KeyFrameLong) a).Interpolate(t, (KeyFrameLong) b);
        private static readonly Func<long, KeyFrame, KeyFrame, bool> FuncCalcBool = (t, a, b) => ((KeyFrameBoolean) a).Interpolate(t, (KeyFrameBoolean) b);
        private static readonly Func<long, KeyFrame, KeyFrame, Vector2> FuncCalcVector2 = (t, a, b) => ((KeyFrameVector2) a).Interpolate(t, (KeyFrameVector2) b);
        private readonly List<KeyFrame> keyFrameList;
        private bool isOverrideEnabled;

        /// <summary>
        /// True when this sequence has no key frames, false when key frames are present
        /// </summary>
        public bool IsEmpty => this.keyFrameList.Count < 1;

        /// <summary>
        /// A keyframe that initially stores the default value for our parameter.
        /// It is used when there are no key frames, <see cref="IsOverrideEnabled"/> is true,
        /// or when <see cref="UpdateValue(long)"/> is called with a negative frame
        /// </summary>
        public KeyFrame DefaultKeyFrame { get; }

        /// <summary>
        /// Gets or sets whether or not the current automation sequence is in override mode.
        /// When in override mode, the automation engine does not update the effective value for
        /// the parameter, even if it has key frames, and instead uses <see cref="DefaultKeyFrame"/>
        /// to get/set the underlying value
        /// </summary>
        public bool IsOverrideEnabled {
            get => this.isOverrideEnabled;
            set {
                if (this.isOverrideEnabled == value)
                    return;
                this.isOverrideEnabled = value;
                this.OverrideStateChanged?.Invoke(this);

                if (this.AutomationData.Owner.GetRelativePlayHead(out long playHead)) {
                    if (value) {
                        this.DefaultKeyFrame.AssignCurrentValue(playHead, this, true);
                    }

                    this.UpdateValue(playHead);
                }
            }
        }

        public bool HasKeyFrames => this.keyFrameList.Count > 0;

        /// <summary>
        /// Returns true when <see cref="IsOverrideEnabled"/> is false, and there are key
        /// frames present, meaning the automation engine can operate upon this sequence normally
        /// </summary>
        public bool CanAutomate => !this.IsOverrideEnabled && this.HasKeyFrames;

        /// <summary>
        /// An enumerable of all the key frames, ordered by the timestamp (small to big).
        /// <para>
        /// This list should NOT be modified directly, as it may result in corruption of the automation
        /// engine. Instead, use the methods provided by this class to modify this list
        /// </para>
        /// </summary>
        public List<KeyFrame> KeyFrames => this.keyFrameList;

        public AutomationDataType DataType { get; }

        public AutomationData AutomationData { get; }

        public Parameter Parameter { get; }

        /// <summary>
        /// Returns true while updating the effective value of our data's owner
        /// </summary>
        public bool IsValueChanging { get; private set; }

        /// <summary>
        /// An event fired, notifying any listeners to query their live value from the automation data
        /// </summary>
        public event ParameterChangedEventHandler ParameterChanged;

        public event KeyFrameAddedEventHandler KeyFrameAdded;
        public event KeyFrameRemovedEventHandler KeyFrameRemoved;
        public event AutomationSequenceEventHandler OverrideStateChanged;

        public AutomationSequence(AutomationData automationData, Parameter parameter) {
            this.AutomationData = automationData;
            this.Parameter = parameter;
            this.keyFrameList = new List<KeyFrame>();
            this.DataType = parameter.DataType;
            this.DefaultKeyFrame = parameter.CreateKeyFrame();
            KeyFrame.SetupDefaultKeyFrameForSequence(this.DefaultKeyFrame, this);
        }

        public void Clear() {
            for (int i = this.keyFrameList.Count - 1; i >= 0; i--) {
                this.RemoveKeyFrameAtIndexInternal(i);
            }
        }

        /// <summary>
        /// Updates our automation data owner's effective value based on the state of this sequence, using
        /// the given frame to calculate the value. This method fired <see cref="ParameterChanged"/> after
        /// the effective value has been set, and then finally notifies our <see cref="AutomationData"/>
        /// to invoke its own parameter value changed event
        /// </summary>
        /// <param name="frame">The frame. May be -1, signaling to reset the value to default</param>
        public void UpdateValue(long frame) {
            if (this.IsValueChanging) {
                throw new InvalidOperationException("Already updating the value");
            }

            try {
                this.IsValueChanging = true;
                this.Parameter.SetValue(this, frame);
                this.ParameterChanged?.Invoke(this);
                AutomationData.InternalOnParameterValueChanged(this);
                if ((this.Parameter.Flags & ParameterFlags.AffectsRender) != 0) {
                    this.InvalidateTimelineRender();
                }
            }
            finally {
                this.IsValueChanging = false;
            }
        }

        /// <summary>
        /// Tries to query our automation data owner's relative frame and checks if it is in range of the automatable
        /// object. If it is, then <see cref="UpdateValue(long)"/> is called to update its effective value
        /// </summary>
        public void UpdateValue() {
            // TODO: this feels really bad...
            IAutomatable owner = this.AutomationData.Owner;
            Timeline timeline = owner.Timeline;
            if (timeline != null) {
                long frame = timeline.PlayHeadPosition;
                bool isValid = true;
                if (owner is IStrictFrameRange strict) {
                    frame = strict.ConvertTimelineToRelativeFrame(frame, out isValid);
                }

                if (isValid) {
                    this.UpdateValue(frame);
                }
            }
        }

        /// <summary>
        /// Accesses our automation data's owner's timeline and invalidates its render. Does nothing
        /// if the timeline is null (e.g. clip or track is not in a timeline yet)
        /// </summary>
        public void InvalidateTimelineRender() => this.AutomationData.InvalidateTimelineRender();

        #region Helper Getter Functions

        public float GetFloatValue(long frame, bool ignoreOverrideState = false) {
            ValidateType(AutomationDataType.Float, this.DataType);
            return this.GetValueInternal(frame, FuncGetFloat, FuncCalcFloat, ignoreOverrideState);
        }

        public double GetDoubleValue(long frame, bool ignoreOverrideState = false) {
            ValidateType(AutomationDataType.Double, this.DataType);
            return this.GetValueInternal(frame, FuncGetDouble, FuncCalcDouble, ignoreOverrideState);
        }

        public long GetLongValue(long frame, bool ignoreOverrideState = false) {
            ValidateType(AutomationDataType.Long, this.DataType);
            return this.GetValueInternal(frame, FuncGetLong, FuncCalcLong, ignoreOverrideState);
        }

        public bool GetBooleanValue(long frame, bool ignoreOverrideState = false) {
            ValidateType(AutomationDataType.Boolean, this.DataType);
            return this.GetValueInternal(frame, FuncGetBool, FuncCalcBool, ignoreOverrideState);
        }

        public Vector2 GetVector2Value(long frame, bool ignoreOverrideState = false) {
            ValidateType(AutomationDataType.Vector2, this.DataType);
            return this.GetValueInternal(frame, FuncGetVector2, FuncCalcVector2, ignoreOverrideState);
        }

        private T GetValueInternal<T>(long frame, Func<KeyFrame, T> toValue, Func<long, KeyFrame, KeyFrame, T> interpolate, bool ignoreOverride = false) {
            if ((ignoreOverride || !this.IsOverrideEnabled) && this.GetIndicesForInterpolation(frame, out int a, out int b)) {
                return b == -1 ? toValue(this.keyFrameList[a]) : interpolate(frame, this.keyFrameList[a], this.keyFrameList[b]);
            }
            else {
                return toValue(this.DefaultKeyFrame);
            }
        }

        #endregion

        /// <summary>
        /// Gets the indices of the key frames, a single key frame if that's all that is possible or
        /// logical, or none, that should be interpolated between to get a final effective value
        /// <list type="bullet">
        ///     <item>
        ///         If the time directly intersects a key frame, then the last keyframe that intersects frame will be set as a, and b will be -1
        ///     </item>
        ///     <item>
        ///         If the time is before the first key frame or after the last key frame, the first or last key frame index is set as a, and b will be -1
        ///     </item>
        ///     <item>
        ///         If all other cases are false, and the list is not empty, then a and b will point to a pair of key frames that frame can be interpolated between (based on a's interpolation method)
        ///     </item>
        ///     <item>
        ///         Otherwise, the method returns false when the list is empty or frame is a negative number
        ///     </item>
        /// </list>
        /// </summary>
        /// <param name="frame">The time</param>
        /// <param name="a">The first or only index available</param>
        /// <param name="b">The second key frame index, may be -1 under certain conditions, in which case use a</param>
        /// <returns>False if there are no key frames, otherwise true</returns>
        public bool GetIndicesForInterpolation(long frame, out int a, out int b) {
            List<KeyFrame> list;
            int count;
            if (frame < 0 || (count = (list = this.keyFrameList).Count) < 1) {
                a = b = -1;
                return false;
            }

            int lhs = 0, rhs = count - 1;
            while (lhs <= rhs) {
                int mid = (lhs + rhs) / 2;
                KeyFrame value = list[mid];
                if (frame > value.Frame) {
                    lhs = mid + 1;
                }
                else if (frame < value.Frame) {
                    rhs = mid - 1;
                }
                else {
                    // find last matching timestamp
                    int j = mid + 1;
                    while (j < count && list[j].Frame == frame)
                        j++;
                    a = j - 1;
                    b = j < count ? j : -1;
                    return true;
                }
            }

            // no intersecting key frame found... figure out interpolation
            if (rhs < 0) {
                a = 0;
                b = -1;
            }
            else if (lhs >= count) {
                a = count - 1;
                b = -1;
            }
            else {
                a = rhs;
                b = lhs;
            }

            return true;
        }

        public int GetIndexOf(KeyFrame keyFrame) {
            if (keyFrame == null)
                throw new ArgumentNullException(nameof(keyFrame));

            List<KeyFrame> list = this.keyFrameList;
            int count = list.Count;
            long frame = keyFrame.Frame;
            int lhs = 0, rhs = count - 1;
            while (lhs <= rhs) {
                int mid = (lhs + rhs) / 2;
                KeyFrame value = list[mid];
                if (frame > value.Frame) {
                    lhs = mid + 1;
                }
                else if (frame < value.Frame) {
                    rhs = mid - 1;
                }
                else if (value == keyFrame) {
                    return mid;
                }
                else { // frame matches; scan until reference found
                    int j = mid + 1;
                    while (j < count && (value = list[j]).Frame == frame && value != keyFrame)
                        j++;
                    if (j == count)
                        return -1;
                    return j;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the index of the last <see cref="KeyFrame"/> at the given frame. Key frames are ordered left to
        /// right, but in the vertical axis, it is unordered, so return the index of the last one at the frame
        /// </summary>
        /// <param name="frame">Target frame</param>
        /// <returns>The index of the last key frame at the given frame, or -1 if there are no key frames at the given frame</returns>
        public int GetLastFrameExactlyAt(long frame) {
            // Do binary search until a matching timestamp, then do a linear search
            // towards the end of the list to find the last matching timestamp
            List<KeyFrame> list = this.keyFrameList;
            int lhs = 0, rhs, k = rhs = list.Count - 1;
            while (lhs <= rhs) {
                int i = lhs + (rhs - lhs) / 2;
                KeyFrame keyFrame = list[i];
                if (keyFrame.Frame == frame) {
                    while (i < k && list[i + 1].Frame == frame)
                        i++;
                    return i;
                }
                else if (keyFrame.Frame < frame) {
                    lhs = i + 1;
                }
                else {
                    rhs = i - 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Inserts the given key frame based on its timestamp, and returns the index that it was inserted at
        /// </summary>
        /// <param name="keyFrame">The key frame to add</param>
        /// <returns>The index of the key frame</returns>
        /// <exception cref="ArgumentException">Timestamp is negative or the data type is invalid</exception>
        public int AddKeyFrame(KeyFrame keyFrame) {
            if (keyFrame.Frame < 0)
                throw new ArgumentException("Keyframe time stamp must be non-negative: " + keyFrame.Frame, nameof(keyFrame));
            if (keyFrame.DataType != this.DataType)
                throw new ArgumentException($"Invalid key frame data type. Expected {this.DataType}, got {keyFrame.DataType}", nameof(keyFrame));
            if (keyFrame.sequence != null && keyFrame.sequence.GetIndexOf(keyFrame) != -1)
                throw new InvalidOperationException("Key frame already exists in another sequence");
            return this.AddKeyFrameInternal(keyFrame);
        }

        /// <summary>
        /// Creates a new key frame, setting its frame to the given frame and inserts it into this sequence, returning the index of its insertion
        /// </summary>
        /// <param name="frame">The location of the key frame</param>
        /// <param name="keyFrame">The key frame that was created</param>
        /// <returns>The index of insertion</returns>
        /// <exception cref="ArgumentException">The frame was negative</exception>
        public int AddNewKeyFrame(long frame, out KeyFrame keyFrame) {
            if (frame < 0)
                throw new ArgumentException("Keyframe time stamp must be non-negative: " + frame, nameof(frame));
            keyFrame = this.Parameter.CreateKeyFrame(frame);
            return this.AddKeyFrameInternal(keyFrame);
        }

        private int AddKeyFrameInternal(KeyFrame keyFrame) {
            long dstFrame = keyFrame.Frame;
            keyFrame.sequence = this;

            // iterate backwards (largest to smallest frame) and insert somewhere good
            List<KeyFrame> list = this.keyFrameList;
            for (int i = list.Count - 1; i >= 0; i--) {
                if (dstFrame >= list[i].Frame) {
                    this.InsertInternal(i + 1, keyFrame);
                    return i + 1;
                }
            }

            this.InsertInternal(0, keyFrame);
            return 0;
        }

        private void InsertInternal(int index, KeyFrame keyFrame) {
            this.keyFrameList.Insert(index, keyFrame);
            this.KeyFrameAdded?.Invoke(this, keyFrame, index);
        }

        public bool RemoveKeyFrame(KeyFrame keyFrame, out int oldIndex) {
            int index = this.GetIndexOf(keyFrame);
            if (index == -1) {
                oldIndex = index;
                return false;
            }

            this.RemoveKeyFrameAtIndexInternal(index);
            oldIndex = index;
            return true;
        }

        /// <summary>
        /// Unsafely removes the key frame at the given index
        /// </summary>
        private void RemoveKeyFrameAtIndexInternal(int index) {
            KeyFrame keyFrame = this.keyFrameList[index];
            keyFrame.sequence = null;
            this.keyFrameList.RemoveAt(index);
            this.KeyFrameRemoved?.Invoke(this, keyFrame, index);
        }

        /// <summary>
        /// Gets a key frame at the raw index
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The key frame</returns>
        public KeyFrame GetKeyFrameAtIndex(int index) {
            return this.keyFrameList[index];
        }

        /// <summary>
        /// Gets or creates a key frame at the given frame. This returns the last
        /// key frame if there are multiple key frames at the given frame.
        /// </summary>
        /// <param name="frame">The key frame time</param>
        /// <param name="assignCurrentValue">
        /// When creating a key frame: true to calculate the current value at the frame and assign
        /// the key frame's value to it, otherwise false to use our parameter's default value
        /// </param>
        /// <returns>A non-null key frame</returns>
        public KeyFrame GetOrCreateKeyFrameAtFrame(long frame, out int index, bool assignCurrentValue = false) {
            KeyFrame keyFrame;
            if ((index = this.GetLastFrameExactlyAt(frame)) == -1) {
                // object value = assignCurrentValue ? this.GetObjectValue(frame, true) : null;
                object value = assignCurrentValue ? this.Parameter.GetObjectValue(frame, this) : null;
                index = this.AddNewKeyFrame(frame, out keyFrame);
                if (assignCurrentValue) {
                    keyFrame.SetValueFromObject(value);
                }
            }
            else {
                keyFrame = this.keyFrameList[index];
            }

            return keyFrame;
        }

        // read/write operations are used for cloning as well as reading from disk

        public void WriteToRBE(RBEDictionary data) {
            data.SetByte(nameof(this.DataType), (byte) this.DataType);
            data.SetBool(nameof(this.IsOverrideEnabled), this.IsOverrideEnabled);
            this.DefaultKeyFrame.WriteToRBE(data.CreateDictionary(nameof(this.DefaultKeyFrame)));

            RBEList list = data.CreateList(nameof(this.KeyFrames));
            foreach (KeyFrame keyFrame in this.keyFrameList) {
                // when reading, use key's DataType to create new key frames and hope the types are correct
                keyFrame.WriteToRBE(list.AddDictionary());
            }
        }

        public void ReadFromRBE(RBEDictionary data) {
            AutomationDataType type = (AutomationDataType) data.GetByte(nameof(this.DataType));
            if (type != this.DataType) {
                throw new Exception($"Data and current instance data type mis-match: {type} != {this.DataType}");
            }

            this.IsOverrideEnabled = data.GetBool(nameof(this.IsOverrideEnabled), false);
            this.DefaultKeyFrame.ReadFromRBE(data.GetDictionary(nameof(this.DefaultKeyFrame)));

            List<KeyFrame> frames = new List<KeyFrame>();
            RBEList list = data.GetList(nameof(this.KeyFrames));
            foreach (RBEDictionary rbe in list.Cast<RBEDictionary>()) {
                KeyFrame keyFrame = this.Parameter.CreateKeyFrame();
                keyFrame.ReadFromRBE(rbe);
                frames.Add(keyFrame);
            }

            // just in case they somehow end up unordered
            frames.Sort((a, b) => a.Frame.CompareTo(b.Frame));
            this.Clear();
            foreach (KeyFrame frame in frames) {
                frame.sequence = this;
                this.keyFrameList.Add(frame);
            }
        }

        public static void LoadDataIntoClone(AutomationSequence src, AutomationSequence dst) {
            if (!src.Parameter.Equals(dst.Parameter)) {
                throw new Exception($"Key mis-match: {src.Parameter} != {dst.Parameter}");
            }

            // slower than manual copy, but safer in terms of updates just in case
            RBEDictionary dictionary = new RBEDictionary();
            src.WriteToRBE(dictionary);
            dst.ReadFromRBE(dictionary);
        }

        public static void ValidateType(AutomationDataType expected, AutomationDataType actual) {
            if (expected != actual) {
                throw new ArgumentException($"Invalid data type. Expected {expected}, got {actual}");
            }
        }

        public override string ToString() {
            return $"{nameof(AutomationSequence)}<{this.Parameter.Key} of type {this.DataType} ({this.keyFrameList.Count} keyframes)>";
        }

        public static void OnKeyFrameValueChanged(AutomationSequence sequence, KeyFrame keyFrame) {
            sequence?.UpdateValue();
        }

        public static void OnKeyFramePositionChanged(AutomationSequence sequence, KeyFrame keyFrame) {
            sequence?.UpdateValue();
        }

        public static void InternalVerifyValue(KeyFrameFloat keyFrame, float value) {
            if (keyFrame.sequence != null && ((ParameterDescriptorFloat) keyFrame.sequence.Parameter.Descriptor).IsValueOutOfRange(value)) {
                throw new ArgumentOutOfRangeException(nameof(value), "Key frame value is out of range");
            }
        }

        public static void InternalVerifyValue(KeyFrameDouble keyFrame, double value) {
            if (keyFrame.sequence != null && ((ParameterDescriptorDouble) keyFrame.sequence.Parameter.Descriptor).IsValueOutOfRange(value)) {
                throw new ArgumentOutOfRangeException(nameof(value), "Key frame value is out of range");
            }
        }

        public static void InternalVerifyValue(KeyFrameLong keyFrame, long value) {
            if (keyFrame.sequence != null && ((ParameterDescriptorLong) keyFrame.sequence.Parameter.Descriptor).IsValueOutOfRange(value)) {
                throw new ArgumentOutOfRangeException(nameof(value), "Key frame value is out of range");
            }
        }

        public static void InternalVerifyValue(KeyFrameVector2 keyFrame, Vector2 value) {
            if (keyFrame.sequence != null && ((ParameterDescriptorVector2) keyFrame.sequence.Parameter.Descriptor).IsValueOutOfRange(value)) {
                throw new ArgumentOutOfRangeException(nameof(value), "Key frame value is out of range");
            }
        }
    }
}