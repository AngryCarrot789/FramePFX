using System;
using System.Collections.Generic;
using System.Numerics;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.RBC;

namespace FramePFX.Core.Automation.Keyframe {
    /// <summary>
    /// Contains all of the key frames for a specific <see cref="AutomationKey"/>
    /// </summary>
    public sealed class AutomationSequence : IRBESerialisable {
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
        /// Whether or not the current automation sequence is in override mode or not. When in override mode,
        /// the automation engine cannot update the value of any parameter, even if it has key frames
        /// </summary>
        public bool IsOverrideEnabled { get; set; }

        public bool HasKeyFrames => this.keyFrameList.Count > 0;

        /// <summary>
        /// Returns true when <see cref="IsOverrideEnabled"/> is false, and there are key frames present, meaning the automation engine is operating in normal operation
        /// </summary>
        public bool IsAutomationInUse => !this.IsOverrideEnabled && this.HasKeyFrames;

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
        /// <param name="engine">The engine that caused this update</param>
        /// <param name="frame">The frame</param>
        public void DoUpdateValue(AutomationEngine engine, long frame) {
            this.UpdateValue?.Invoke(this, frame);
        }

        #region Helper Getter Functions

        // Caching just as a slight performance helper... hopefully. It seems like a new lambda class is creating
        // each time one is used, unless it gets inlined by the JIT... dunno. TBD
        private static readonly Func<KeyFrame, float>   VAL_float =   k => ((KeyFrameFloat) k).Value;
        private static readonly Func<KeyFrame, double>  VAL_double =  k => ((KeyFrameDouble) k).Value;
        private static readonly Func<KeyFrame, long>    VAL_long =    k => ((KeyFrameLong) k).Value;
        private static readonly Func<KeyFrame, bool>    VAL_bool =    k => ((KeyFrameBoolean) k).Value;
        private static readonly Func<KeyFrame, Vector2> VAL_Vector2 = k => ((KeyFrameVector2) k).Value;
        private static readonly Func<long, KeyFrame, KeyFrame, float>   IPOL_float =   (t, a, b) => ((KeyFrameFloat) a).Interpolate(t, (KeyFrameFloat) b);
        private static readonly Func<long, KeyFrame, KeyFrame, double>  IPOL_double =  (t, a, b) => ((KeyFrameDouble) a).Interpolate(t, (KeyFrameDouble) b);
        private static readonly Func<long, KeyFrame, KeyFrame, long>    IPOL_long =    (t, a, b) => ((KeyFrameLong) a).Interpolate(t, (KeyFrameLong) b);
        private static readonly Func<long, KeyFrame, KeyFrame, bool>    IPOL_bool =    (t, a, b) => ((KeyFrameBoolean) a).Interpolate(t, (KeyFrameBoolean) b);
        private static readonly Func<long, KeyFrame, KeyFrame, Vector2> IPOL_Vector2 = (t, a, b) => ((KeyFrameVector2) a).Interpolate(t, (KeyFrameVector2) b);

        public float GetFloatValue(long time) {
            ValidateType(AutomationDataType.Float, this.DataType);
            return this.TryGetValueInternal(time, VAL_float, IPOL_float);
        }

        public double GetDoubleValue(long time) {
            ValidateType(AutomationDataType.Double, this.DataType);
            return this.TryGetValueInternal(time, VAL_double, IPOL_double);
        }

        public long GetLongValue(long time) {
            ValidateType(AutomationDataType.Long, this.DataType);
            return this.TryGetValueInternal(time, VAL_long, IPOL_long);
        }

        public bool GetBooleanValue(long time) {
            ValidateType(AutomationDataType.Boolean, this.DataType);
            return this.TryGetValueInternal(time, VAL_bool, IPOL_bool);
        }

        public Vector2 GetVector2Value(long time) {
            ValidateType(AutomationDataType.Vector2, this.DataType);
            return this.TryGetValueInternal(time, VAL_Vector2, IPOL_Vector2);
        }

        private T TryGetValueInternal<T>(long time, Func<KeyFrame, T> toValue, Func<long, KeyFrame, KeyFrame, T> interpolate) {
            if (this.IsOverrideEnabled || this.IsEmpty) {
                return toValue(this.OverrideKeyFrame);
            }
            else if (this.GetKeyFramesForFrame(time, out KeyFrame a, out KeyFrame b, out int index)) {
                // pass `time` parameter to the interpolate function to remove closure allocation; performance helper
                return b == null ? toValue(a) : interpolate(time, a, b);
            }
            else {
                // this shouldn't occur because the above code checks if there are no key frames
                #if DEBUG
                System.Diagnostics.Debugger.Break();
                #endif
                throw new Exception("???");
            }
        }

        #endregion

        // not used
        public int BinarySearch(long frame) {
            List<KeyFrame> list = this.keyFrameList;
            int i = 0, j = list.Count - 1;
            while (i <= j) {
                int k = i + (j - i >> 1);
                long time = list[k].time;
                if (time == frame) {
                    return k;
                }
                else if (time < frame) {
                    i = k + 1;
                }
                else {
                    j = k - 1;
                }
            }

            return ~i;
        }

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
            List<KeyFrame> list = this.keyFrameList;
            int count = list.Count;
            if (count < 1) {
                a = b = null;
                i = -1;
                return false;
            }

            KeyFrame value = list[i = 0], prev = null;
            while (true) { // node is never null, as it is only reassigned to next (which won't be null at that point)
                long valTime = value.time;
                if (frame > valTime) {
                    if (++i < count) {
                        prev = value;
                        value = list[i];
                    }
                    else { // last key frame
                        a = value;
                        b = null;
                        return true;
                    }
                }
                else if (frame < valTime) {
                    if (prev == null) { // first key frame; time is before the first node
                        a = value;
                        b = null;
                        return true;
                    }
                    else { // found pair of key frames to interpolate between
                        a = prev;
                        b = value;
                        return true;
                    }
                }
                else {
                    // get the last node next node whose timestamp equals the input time, otherwise
                    // use the last node (the input time is the same as the very last node's timestamp)
                    KeyFrame temp = value, tempPrev = prev;
                    while (temp != null && temp.time == frame) {
                        tempPrev = temp;
                        temp = ++i < count ? list[i] : null;
                    }

                    if (temp != null && tempPrev != null) {
                        a = tempPrev;
                        b = temp;
                    }
                    else {
                        a = temp ?? tempPrev;
                        b = null;
                    }

                    return true;
                }
            }
        }

        /*
            implements caching but it breaks sometimes
            public bool GetKeyFramesForFrame(long frame, out KeyFrame a, out KeyFrame b, out int i) {
                List<KeyFrame> list = this.keyFrameList;
                int count = list.Count, j;
                if (count < 1) {
                    a = b = null;
                    i = -1;
                    return false;
                }

                if (this.cache_valid) {
                    if (frame >= this.cache_time) {
                        i = this.cache_index;
                        j = count - 1;
                    }
                    else {
                        i = 0;
                        j = this.cache_index;
                    }
                }
                else {
                    i = 0;
                    j = count - 1;
                }

                KeyFrame value = list[i], prev = null;
                while (true) { // node is never null, as it is only reassigned to next (which won't be null at that point)
                    long valTime = value.Timestamp;
                    if (frame > valTime) {
                        if (++i <= j) {
                            prev = value;
                            value = list[i];
                        }
                        else { // last key frame
                            a = value;
                            b = null;
                            this.cache_valid = true;
                            this.cache_index = i - 1;
                            this.cache_time = frame;
                            return true;
                        }
                    }
                    else if (frame < valTime) {
                        this.cache_valid = true;
                        this.cache_index = i;
                        this.cache_time = frame;
                        if (prev == null) { // first key frame; time is before the first node
                            if (this.cache_valid && this.cache_index > 0) {
                                a = list[this.cache_index - 1];
                                b = value;
                                return true;
                            }

                            a = value;
                            b = null;
                            return true;
                        }
                        else { // found pair of key frames to interpolate between
                            a = prev;
                            b = value;
                            return true;
                        }
                    }
                    else {
                        // get the last node next node whose timestamp equals the input time, otherwise
                        // use the last node (the input time is the same as the very last node's timestamp)
                        KeyFrame temp = value, tempPrev = prev;
                        while (temp != null && temp.Timestamp == frame) {
                            tempPrev = temp;
                            temp = ++i <= j ? list[i] : null;
                        }

                        if (temp != null && tempPrev != null) {
                            a = tempPrev;
                            b = temp;
                        }
                        else {
                            a = temp ?? tempPrev;
                            b = null;
                        }

                        this.cache_valid = true;
                        this.cache_index = i - 1;
                        this.cache_time = frame;
                        return true;
                    }
                }
            }
         */

        /// <summary>
        /// Enumerates all of they keys are are located at the given frame
        /// </summary>
        /// <param name="frame">Target frame</param>
        /// <returns>The key frames at the given frame</returns>
        public IEnumerable<KeyFrame> GetKeyFramesAt(long frame) {
            foreach (KeyFrame keyFrame in this.keyFrameList) {
                long time = keyFrame.time;
                if (time == frame) {
                    yield return keyFrame;
                }
                else if (time > frame) {
                    yield break;
                }
            }
        }

        /// <summary>
        /// Gets the last <see cref="KeyFrame"/> at the given frame. Key frames are ordered left to right, but in the vertical axis, it is unordered
        /// </summary>
        /// <param name="frame">Target frame</param>
        /// <returns>The last key frame at the given frame, or null, if there are no key frames at the given frame</returns>
        public KeyFrame GetLastFrameExactlyAt(long frame) {
            KeyFrame last = null;
            foreach (KeyFrame keyFrame in this.keyFrameList) {
                long time = keyFrame.time;
                if (time == frame) {
                    last = keyFrame;
                }
                else if (time > frame) {
                    break;
                }
            }

            return last;
        }

        /// <summary>
        /// Inserts the given key frame based on its timestamp, and returns the index that it was inserted at
        /// </summary>
        /// <param name="newKeyFrame">The key frame to add</param>
        /// <returns>The index of the key frame</returns>
        /// <exception cref="ArgumentException">Timestamp is negative or the data type is invalid</exception>
        public int AddKeyFrame(KeyFrame newKeyFrame) {
            long timeStamp = newKeyFrame.time;
            if (timeStamp < 0)
                throw new ArgumentException("Keyframe time stamp must be non-negative: " + timeStamp, nameof(newKeyFrame));
            if (newKeyFrame.DataType != this.DataType)
                throw new ArgumentException($"Invalid key frame data type. Expected {this.DataType}, got {newKeyFrame.DataType}", nameof(newKeyFrame));
            newKeyFrame.sequence = this;
            List<KeyFrame> list = this.keyFrameList;
            for (int i = list.Count; i >= 0; i--) {
                if (timeStamp >= list[i - 1].time) {
                    list.Insert(i, newKeyFrame);
                    return i;
                }
            }

            list.Insert(0, newKeyFrame);
            return 0;
        }

        /// <summary>
        /// Unsafely inserts the key frame at the given index, ignoring order. Do not use!
        /// </summary>
        public void InsertKeyFrame(int index, KeyFrame newKeyFrame) {
            long timeStamp = newKeyFrame.time;
            if (timeStamp < 0)
                throw new ArgumentException("Keyframe time stamp must be non-negative: " + timeStamp, nameof(newKeyFrame));
            if (newKeyFrame.DataType != this.DataType)
                throw new ArgumentException($"Invalid key frame data type. Expected {this.DataType}, got {newKeyFrame.DataType}", nameof(newKeyFrame));
            newKeyFrame.sequence = this;
            this.keyFrameList.Insert(index, newKeyFrame);
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
            frames.Sort((a, b) => a.time.CompareTo(b.time));
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
            return $"{nameof(AutomationSequence)}({this.DataType} -> {this.Key.FullId} [{this.keyFrameList.Count} keyframes])";
        }
    }
}