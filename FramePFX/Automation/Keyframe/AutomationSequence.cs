using System;
using System.Collections.Generic;
using System.Numerics;
using FramePFX.Automation.Events;
using FramePFX.Automation.Keys;
using FramePFX.RBC;

namespace FramePFX.Automation.Keyframe {
    /// <summary>
    /// Contains all of the key frames for a specific <see cref="AutomationKey"/>
    /// </summary>
    public class AutomationSequence : IRBESerialisable {
        private static readonly Func<KeyFrame, float> FuncGetFloat = k => ((KeyFrameFloat) k).Value;
        private static readonly Func<KeyFrame, double> FuncGetDouble = k => ((KeyFrameDouble) k).Value;
        private static readonly Func<KeyFrame, long> FuncGetLong = k => ((KeyFrameLong) k).Value;
        private static readonly Func<KeyFrame, bool> FuncGetBool = k => ((KeyFrameBoolean) k).Value;
        private static readonly Func<KeyFrame, Vector2> FuncGetVec2 = k => ((KeyFrameVector2) k).Value;
        private static readonly Func<long, KeyFrame, KeyFrame, float> FuncCalcFloat = (t, a, b) => ((KeyFrameFloat) a).Interpolate(t, (KeyFrameFloat) b);
        private static readonly Func<long, KeyFrame, KeyFrame, double> FuncCalcDouble = (t, a, b) => ((KeyFrameDouble) a).Interpolate(t, (KeyFrameDouble) b);
        private static readonly Func<long, KeyFrame, KeyFrame, long> FuncCalcLong = (t, a, b) => ((KeyFrameLong) a).Interpolate(t, (KeyFrameLong) b);
        private static readonly Func<long, KeyFrame, KeyFrame, bool> FuncCalcBool = (t, a, b) => ((KeyFrameBoolean) a).Interpolate(t, (KeyFrameBoolean) b);
        private static readonly Func<long, KeyFrame, KeyFrame, Vector2> FuncCalcVec2 = (t, a, b) => ((KeyFrameVector2) a).Interpolate(t, (KeyFrameVector2) b);
        private readonly List<KeyFrame> keyFrameList;

        /// <summary>
        /// Whether or not this sequence has any key frames
        /// </summary>
        public bool IsEmpty => this.keyFrameList.Count < 1;

        /// <summary>
        /// A keyframe that stores an override value, which overrides any automation
        /// </summary>
        public KeyFrame OverrideKeyFrame { get; }

        /// <summary>
        /// Gets or sets whether or not the current automation sequence is in override mode.
        /// When in override mode, the automation engine cannot update the value of any parameter, even if it has key frames
        /// </summary>
        public bool IsOverrideEnabled { get; set; }

        public bool HasKeyFrames => this.keyFrameList.Count > 0;

        /// <summary>
        /// Returns true when <see cref="IsOverrideEnabled"/> is false, and there are key
        /// frames present, meaning the automation engine can operate upon this sequence normally
        /// </summary>
        public bool IsAutomationReady => !this.IsOverrideEnabled && this.HasKeyFrames;

        /// <summary>
        /// An enumerable of all the key frames, ordered by the timestamp (small to big)
        /// </summary>
        public IEnumerable<KeyFrame> KeyFrames => this.keyFrameList;

        public AutomationDataType DataType { get; }

        public AutomationData AutomationData { get; }

        public AutomationKey Key { get; }

        /// <summary>
        /// An event fired, notifying any listeners to query their live value from the automation data
        /// </summary>
        public UpdateAutomationValueEventHandler UpdateValue;

        public AutomationSequence(AutomationData automationData, AutomationKey key) {
            this.AutomationData = automationData;
            this.Key = key;
            this.keyFrameList = new List<KeyFrame>();
            this.DataType = key.DataType;
            this.OverrideKeyFrame = key.CreateKeyFrame();
            this.OverrideKeyFrame.sequence = this;
        }

        public void Clear() {
            foreach (KeyFrame keyFrame in this.keyFrameList) {
                keyFrame.sequence = null;
            }

            this.keyFrameList.Clear();
        }

        /// <summary>
        /// Invokes the <see cref="UpdateValue"/> event, allowing any listeners to re-query their actual value at the given frame
        /// </summary>
        /// <param name="frame">The frame</param>
        public void DoUpdateValue(long frame) {
            this.UpdateValue?.Invoke(this, frame);
        }

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
            return this.GetValueInternal(frame, FuncGetVec2, FuncCalcVec2, ignoreOverrideState);
        }

        private T GetValueInternal<T>(long frame, Func<KeyFrame, T> toValue, Func<long, KeyFrame, KeyFrame, T> interpolate, bool ignoreOverride = false) {
            if ((ignoreOverride || !this.IsOverrideEnabled) && this.GetIndicesForFrame(frame, out int a, out int b)) {
                return b == -1 ? toValue(this.keyFrameList[a]) : interpolate(frame, this.keyFrameList[a], this.keyFrameList[b]);
            }
            else {
                return toValue(this.OverrideKeyFrame);
            }
        }

        #endregion

        /// <summary>
        /// Gets the two key frames that the given time should attempt to interpolate between, or a single key frame if that's all that is possible or logical
        /// <para>
        /// If the time directly intersects a key frame, then the last keyframe that intersects will be set as a, and b will be null (therefore, use a's value directly)
        /// </para>
        /// <para>
        /// If the time is before the first key frame or after the last key frame, the first/last key frame is set as a, and b will be null (therefore, use a's value directly)
        /// </para>
        /// <para>
        /// If all other cases are false, and the list is not empty, a pair of key frames will be available to interpolate between (based on a's interpolation method)
        /// </para>
        /// </summary>
        /// <param name="frame">The time</param>
        /// <param name="a">The first (or only available) key frame</param>
        /// <param name="b">The second key frame, may be null under certain conditions, in which case use a's value directly</param>
        /// <returns>False if there are no key frames, otherwise true</returns>
        public bool GetKeyFramesForFrame(long frame, out KeyFrame a, out KeyFrame b, out int i) {
            if (this.GetIndicesForFrame(frame, out i, out int j)) {
                a = this.keyFrameList[i];
                b = j == -1 ? null : this.keyFrameList[j];
                return true;
            }

            a = b = null;
            return false;
        }

        /// <summary>
        /// Gets the two indices of the key frames that the given time should attempt to interpolate between, or a single key frame if that's all that is possible or logical
        /// <para>
        /// If the time directly intersects a key frame, then the last keyframe that intersects frame will be set as a, and b will be -1
        /// </para>
        /// <para>
        /// If the time is before the first key frame or after the last key frame, the first/last key frame index is set as a, and b will be -1
        /// </para>
        /// <para>
        /// If all other cases are false, and the list is not empty, then a and b will point to a pair of key frames that frame can be interpolated between (based on a's interpolation method)
        /// </para>
        /// </summary>
        /// <param name="frame">The time</param>
        /// <param name="a">The first or only index available</param>
        /// <param name="b">The second key frame index, may be -1 under certain conditions, in which case use a</param>
        /// <returns>False if there are no key frames, otherwise true</returns>
        public bool GetIndicesForFrame(long frame, out int a, out int b) {
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
                if (frame > value.frame) {
                    lhs = mid + 1;
                }
                else if (frame < value.frame) {
                    rhs = mid - 1;
                }
                else {
                    // find last matching timestamp
                    int j = mid + 1;
                    while (j < count && list[j].frame == frame)
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

        /// <summary>
        /// Gets the index of the last <see cref="KeyFrame"/> at the given frame. Key frames are ordered left to
        /// right, but in the vertical axis, it is unordered, so return the index of the last one at the frame
        /// </summary>
        /// <param name="frame">Target frame</param>
        /// <returns>The last key frame at the given frame, or null, if there are no key frames at the given frame</returns>
        public int GetLastFrameExactlyAt(long frame) {
            // Do binary search until a matching timestamp, then do a linear search
            // towards the end of the list to find the last matching timestamp
            List<KeyFrame> list = this.keyFrameList;
            int lhs = 0, rhs, k = rhs = list.Count - 1;
            while (lhs <= rhs) {
                int i = lhs + (rhs - lhs) / 2;
                KeyFrame keyFrame = list[i];
                if (keyFrame.frame == frame) {
                    while (i < k && list[i + 1].frame == frame)
                        i++;
                    return i;
                }
                else if (keyFrame.frame < frame) {
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
            long timeStamp = keyFrame.frame;
            if (timeStamp < 0)
                throw new ArgumentException("Keyframe time stamp must be non-negative: " + timeStamp, nameof(keyFrame));
            if (keyFrame.DataType != this.DataType)
                throw new ArgumentException($"Invalid key frame data type. Expected {this.DataType}, got {keyFrame.DataType}", nameof(keyFrame));
            keyFrame.sequence = this;
            List<KeyFrame> list = this.keyFrameList;
            for (int i = list.Count - 1; i >= 0; i--) {
                if (timeStamp >= list[i].frame) {
                    list.Insert(i + 1, keyFrame);
                    return i + 1;
                }
            }

            list.Insert(0, keyFrame);
            return 0;
        }

        /// <summary>
        /// Unsafely inserts the key frame at the given index, ignoring order. Do not use!
        /// </summary>
        public void InsertKeyFrame(int index, KeyFrame keyFrame) {
            if (keyFrame.frame < 0)
                throw new ArgumentException("Keyframe time stamp must be non-negative: " + keyFrame.frame, nameof(keyFrame));
            if (keyFrame.DataType != this.DataType)
                throw new ArgumentException($"Invalid key frame data type. Expected {this.DataType}, got {keyFrame.DataType}", nameof(keyFrame));
            keyFrame.sequence = this;
            this.keyFrameList.Insert(index, keyFrame);
        }

        /// <summary>
        /// Unsafely removes the key frame at the given index
        /// </summary>
        public void RemoveKeyFrame(int index) {
            this.keyFrameList[index].sequence = null;
            this.keyFrameList.RemoveAt(index);
        }

        /// <summary>
        /// Gets the key frame at the given index
        /// </summary>
        public KeyFrame GetKeyFrameAtIndex(int index) {
            return this.keyFrameList[index];
        }

        // read/write operations are used for cloning as well as reading from disk

        public void WriteToRBE(RBEDictionary data) {
            data.SetByte(nameof(this.DataType), (byte) this.DataType);
            data.SetBool(nameof(this.IsOverrideEnabled), this.IsOverrideEnabled);
            this.OverrideKeyFrame.WriteToRBE(data.CreateDictionary(nameof(this.OverrideKeyFrame)));

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
            this.OverrideKeyFrame.ReadFromRBE(data.GetDictionary(nameof(this.OverrideKeyFrame)));

            List<KeyFrame> frames = new List<KeyFrame>();
            RBEList list = data.GetList(nameof(this.KeyFrames));
            foreach (RBEDictionary rbe in list.OfType<RBEDictionary>()) {
                KeyFrame keyFrame = this.Key.CreateKeyFrame();
                keyFrame.ReadFromRBE(rbe);
                frames.Add(keyFrame);
            }

            // just in case they somehow end up unordered
            frames.Sort((a, b) => a.frame.CompareTo(b.frame));
            this.Clear();
            foreach (KeyFrame frame in frames) {
                frame.sequence = this;
                this.keyFrameList.Add(frame);
            }
        }

        public static void LoadDataIntoClone(AutomationSequence src, AutomationSequence dst) {
            if (src.Key != dst.Key) {
                throw new Exception($"Key mis-match: {src.Key} != {dst.Key}");
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
            return $"{nameof(AutomationSequence)}[{this.Key.FullId} of type {this.DataType} ({this.keyFrameList.Count} keyframes)]";
        }
    }
}