using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Numerics;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.RBC;

namespace FramePFX.Core.Automation.Keyframe {
    /// <summary>
    /// Contains all of the key frames for a specific <see cref="AutomationKey"/>
    /// </summary>
    public class AutomationSequence : IRBESerialisable {
        // private readonly LinkedList<KeyFrame> keyFrameLinkedList;
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
        /// <para>
        /// Alternative name: IsDisabled
        /// </para>
        /// </summary>
        public bool IsOverrideEnabled { get; set; }

        /// <summary>
        /// Returns true when <see cref="IsOverrideEnabled"/> is false, and there are key frames present, meaning the automation engine is operating in normal operation
        /// </summary>
        public bool IsAutomationInUse => !this.IsOverrideEnabled && this.keyFrameList.Count > 0;

        /// <summary>
        /// An enumerable of all the key frames, ordered by the timestamp (small to big)
        /// </summary>
        public IEnumerable<KeyFrame> KeyFrames => this.keyFrameList;

        public AutomationDataType DataType { get; }

        public AutomationKey Key { get; }

        /// <summary>
        /// An event fired, notifying any listeners to query their live value from the automation data
        /// </summary>
        public UpdateAutomationValueEventHandler UpdateValue;

        public AutomationSequence(AutomationKey key) {
            this.Key = key;
            // this.keyFrameLinkedList = new LinkedList<KeyFrame>();
            this.keyFrameList = new List<KeyFrame>();
            this.DataType = key.DataType;
            this.OverrideKeyFrame = key.CreateKeyFrame();
            this.OverrideKeyFrame.OwnerSequence = this;
        }

        public bool TryGetDoubleValue(long time, out double value) {
            ValidateType(AutomationDataType.Double, this.DataType);
            return this.TryGetValueInternal(time, x => ((KeyFrameDouble) x).Value, (t, a, b) => ((KeyFrameDouble) a).Interpolate(t, (KeyFrameDouble) b), out value);
        }

        public double GetDoubleValue(long time) {
            ValidateType(AutomationDataType.Double, this.DataType);
            return this.TryGetDoubleValue(time, out double value) ? value : ((KeyDescriptorDouble) this.Key.Descriptor).DefaultValue;
        }

        public bool TryGetLongValue(long time, out long value) {
            ValidateType(AutomationDataType.Long, this.DataType);
            return this.TryGetValueInternal(time, x => ((KeyFrameLong) x).Value, (t, a, b) => ((KeyFrameLong) a).Interpolate(t, (KeyFrameLong) b), out value);
        }

        public long GetLongValue(long time) {
            ValidateType(AutomationDataType.Long, this.DataType);
            return this.TryGetLongValue(time, out long value) ? value : ((KeyDescriptorLong) this.Key.Descriptor).DefaultValue;
        }

        public bool TryGetBooleanValue(long time, out bool value) {
            ValidateType(AutomationDataType.Boolean, this.DataType);
            return this.TryGetValueInternal(time, x => ((KeyFrameBoolean) x).Value, (t, a, b) => ((KeyFrameBoolean) a).Interpolate(t, (KeyFrameBoolean) b), out value);
        }

        public bool GetBooleanValue(long time) {
            ValidateType(AutomationDataType.Boolean, this.DataType);
            return this.TryGetBooleanValue(time, out bool value) ? value : ((KeyDescriptorBoolean) this.Key.Descriptor).DefaultValue;
        }

        public bool TryGetVector2Value(long time, out Vector2 value) {
            ValidateType(AutomationDataType.Vector2, this.DataType);
            return this.TryGetValueInternal(time, x => ((KeyFrameVector2) x).Value, (t, a, b) => ((KeyFrameVector2) a).Interpolate(t, (KeyFrameVector2) b), out value);
        }

        public Vector2 GetVector2Value(long time) {
            ValidateType(AutomationDataType.Vector2, this.DataType);
            return this.TryGetVector2Value(time, out Vector2 value) ? value : ((KeyDescriptorVector2) this.Key.Descriptor).DefaultValue;
        }

        private bool TryGetValueInternal<T>(long time, Func<KeyFrame, T> toValue, Func<long, KeyFrame, KeyFrame, T> interpolate, out T value) {
            if (this.IsOverrideEnabled || this.IsEmpty) {
                value = toValue(this.OverrideKeyFrame);
                return true;
            }

            if (!this.GetKeyFramesForFrame(time, out KeyFrame a, out KeyFrame b)) {
                value = default;
                return false;
            }

            // pass `time` parameter to the interpolate function to remove closure allocation; performance helper
            value = b == null ? toValue(a) : interpolate(time, a, b);
            return true;
        }

        // TODO: maybe optimise this; cache the head node and last frame/time and only search starting there?

        public bool GetKeyFramesForFrame(long frame, out KeyFrame a, out KeyFrame b) {
            List<KeyFrame> list = this.keyFrameList;
            int i = 0, count = list.Count;
            if (count < 1) {
                a = b = null;
                return false;
            }

            KeyFrame value = list[0], prev = null;
            while (true) { // node is never null, as it is only reassigned to next (which won't be null at that point)
                long nodeTime = value.Timestamp;
                if (frame > nodeTime) {
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
                else if (frame < nodeTime) {
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
                    while (temp != null && temp.Timestamp == frame) {
                        tempPrev = temp;
                        temp = ++i < count ? list[i] : null;
                    }

                    if (temp != null && tempPrev != null) {
                        a = tempPrev;
                        b = temp;
                    }
                    else if (temp != null) {
                        a = temp;
                        b = null;
                    }
                    else {
                        a = tempPrev;
                        b = null;
                    }

                    return true;
                }
            }
        }

        // /// <summary>
        // /// Gets the two key frames that the given time should attempt to interpolate between, or a single key frame if that's all that is possible or logical
        // /// <para>
        // /// If the time directly intersects a key frame, then the last keyframe that intersects will be set as a, and b will be null (therefore, use a's value directly)
        // /// </para>
        // /// <para>
        // /// If the time is before the first key frame or after the last key frame, the first/last key frame is set as a, and b will be null (therefore, use a's value directly)
        // /// </para>
        // /// <para>
        // /// If all other cases are false, and the list is not empty, a pair of key frames will be available to interpolate between (based on a's interpolation method)
        // /// </para>
        // /// </summary>
        // /// <param name="time">The time</param>
        // /// <param name="a">The first (or only available) key frame</param>
        // /// <param name="b">The second key frame, may be null under certain conditions, in which case use a's value directly</param>
        // /// <returns>False if there are no key frames, otherwise true</returns>
        // public bool GetKeyFramesForTime(long time, out KeyFrame a, out KeyFrame b) {
        //     LinkedListNode<KeyFrame> node = this.keyFrameLinkedList.First;
        //     if (node == null) {
        //         a = b = null;
        //         return false;
        //     }
        //
        //     LinkedListNode<KeyFrame> prev = null;
        //     while (true) { // node is never null, as it is only reassigned to next (which won't be null at that point)
        //         long nodeTime = node.Value.Timestamp;
        //         if (time > nodeTime) {
        //             LinkedListNode<KeyFrame> next = node.Next;
        //             if (next == null) { // last key frame
        //                 a = node.Value;
        //                 b = null;
        //                 return true;
        //             }
        //             else {
        //                 prev = node;
        //                 node = next;
        //             }
        //         }
        //         else if (time < nodeTime) {
        //             if (prev == null) { // first key frame; time is before the first node
        //                 a = node.Value;
        //                 b = null;
        //                 return true;
        //             }
        //             else { // found pair of key frames to interpolate between
        //                 a = prev.Value;
        //                 b = node.Value;
        //                 return true;
        //             }
        //         }
        //         else {
        //             // get the last node next node whose timestamp equals the input time, otherwise
        //             // use the last node (the input time is the same as the very last node's timestamp)
        //             LinkedListNode<KeyFrame> temp = node, tempPrev = prev;
        //             while (temp != null && temp.Value.Timestamp == time) {
        //                 tempPrev = temp;
        //                 temp = temp.Next;
        //             }
        //
        //             if (temp != null && tempPrev != null) {
        //                 a = tempPrev.Value;
        //                 b = temp.Value;
        //             }
        //             else if (temp != null) {
        //                 a = temp.Value;
        //                 b = null;
        //             }
        //             else {
        //                 a = tempPrev.Value;
        //                 b = null;
        //             }
        //
        //             return true;
        //         }
        //     }
        // }

        public IEnumerable<KeyFrame> GetFrameExactlyAt(long frame) {
            foreach (KeyFrame keyFrame in this.keyFrameList) {
                long time = keyFrame.Timestamp;
                if (time == frame) {
                    yield return keyFrame;
                }
                else if (time > frame) {
                    yield break;
                }
            }
        }

        public KeyFrame GetLastFrameExactlyAt(long frame) {
            KeyFrame last = null;
            foreach (KeyFrame keyFrame in this.keyFrameList) {
                long time = keyFrame.Timestamp;
                if (time == frame) {
                    last = keyFrame;
                }
                else if (time > frame) {
                    return last;
                }
            }

            return null;
        }

        public static void ValidateType(AutomationDataType expected, AutomationDataType actual) {
            if (expected != actual) {
                throw new ArgumentException($"Invalid data time. Expected {expected}, got {actual}");
            }
        }

        public void AddKeyFrame(KeyFrame newKeyFrame) {
            long timeStamp = newKeyFrame.Timestamp;
            if (timeStamp < 0)
                throw new ArgumentException("Keyframe time stamp must be non-negative: " + timeStamp, nameof(newKeyFrame));
            if (newKeyFrame.DataType != this.DataType)
                throw new ArgumentException($"Invalid key frame data type. Expected {this.DataType}, got {newKeyFrame.DataType}", nameof(newKeyFrame));

            newKeyFrame.OwnerSequence = this;
            List<KeyFrame> list = this.keyFrameList;
            for (int i = 0, c = list.Count; i < c; i++) {
                KeyFrame keyFrame = list[i];
                if (timeStamp < keyFrame.Timestamp) {
                    list.Insert(i, newKeyFrame);
                }
                else if (timeStamp == keyFrame.Timestamp) {
                    list.Insert(i + 1, newKeyFrame);
                }
                else {
                    continue;
                }

                return;
            }

            list.Add(newKeyFrame);
        }

        public void InsertKeyFrame(int index, KeyFrame newKeyFrame) {
            long timeStamp = newKeyFrame.Timestamp;
            if (timeStamp < 0)
                throw new ArgumentException("Keyframe time stamp must be non-negative: " + timeStamp, nameof(newKeyFrame));
            if (newKeyFrame.DataType != this.DataType)
                throw new ArgumentException($"Invalid key frame data type. Expected {this.DataType}, got {newKeyFrame.DataType}", nameof(newKeyFrame));

            newKeyFrame.OwnerSequence = this;
            this.keyFrameList.Insert(index, newKeyFrame);
        }

        public bool RemoveKeyFrame(KeyFrame frame) {
            for (int i = 0; i < this.keyFrameList.Count; i++) {
                KeyFrame keyFrame = this.keyFrameList[i];
                if (ReferenceEquals(keyFrame, frame)) {
                    keyFrame.OwnerSequence = null;
                    this.keyFrameList.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void RemoveKeyFrame(int index) {
            this.keyFrameList[index].OwnerSequence = null;
            this.keyFrameList.RemoveAt(index);
        }

        public void WriteToRBE(RBEDictionary data) {
            data.SetByte(nameof(this.DataType), (byte) this.DataType);
            data.SetBool(nameof(this.IsOverrideEnabled), this.IsOverrideEnabled);
            this.OverrideKeyFrame.WriteToRBE(data.CreateDictionary(nameof(this.OverrideKeyFrame)));

            RBEList list = data.CreateList(nameof(this.KeyFrames));
            foreach (KeyFrame keyFrame in this.keyFrameList) {
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
            foreach (RBEDictionary rbe in list.GetDictionaries()) {
                KeyFrame keyFrame = this.Key.CreateKeyFrame();
                keyFrame.ReadFromRBE(rbe);
                frames.Add(keyFrame);
            }

            frames.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
            foreach (KeyFrame frame in frames) {
                frame.OwnerSequence = this;
                this.keyFrameList.Add(frame);
            }
        }

        public AutomationSequence Clone() {
            RBEDictionary data = new RBEDictionary();
            this.WriteToRBE(data);
            AutomationSequence seq = new AutomationSequence(this.Key);
            seq.ReadFromRBE(data);
            return seq;
        }

        public void DoUpdateValue(AutomationEngine engine, long frame) {
            this.UpdateValue?.Invoke(this, frame);
        }
    }
}