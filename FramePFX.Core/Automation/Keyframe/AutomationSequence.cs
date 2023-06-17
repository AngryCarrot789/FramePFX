using System;
using System.Collections.Generic;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.RBC;

namespace FramePFX.Core.Automation.Keyframe {
    /// <summary>
    /// Contains all of the key frames for a specific <see cref="AutomationKey"/>
    /// </summary>
    public class AutomationSequence : IRBESerialisable {
        private readonly LinkedList<KeyFrame> keyFrames;

        public AutomationKey Key { get; }

        public IEnumerable<KeyFrame> KeyFrames => this.keyFrames;

        public AutomationDataType DataType { get; }

        public AutomationSequence(AutomationKey key) {
            this.Key = key;
            this.keyFrames = new LinkedList<KeyFrame>();
            this.DataType = key.DataType;
        }

        public double GetDoubleValue(long timestamp) {
            ValidateType(AutomationDataType.Double, this.DataType);
            return 0d;
        }

        private static void ValidateType(AutomationDataType expected, AutomationDataType actual) {
            if (expected != actual) {
                throw new ArgumentException($"Invalid data time. Expected {expected}, got {actual}");
            }
        }

        public void AddKeyFrame(KeyFrame keyFrame) {
            long timeStamp = keyFrame.Timestamp;
            if (timeStamp < 0) {
                throw new ArgumentException("Keyframe time stamp must be non-negative: " + timeStamp, nameof(keyFrame));
            }

            if (keyFrame.DataType != this.DataType) {
                throw new ArgumentException($"Invalid key frame data type. Expected {this.DataType}, got {keyFrame.DataType}", nameof(keyFrame));
            }

            for (LinkedListNode<KeyFrame> node = this.keyFrames.First; node != null; node = node.Next) {
                KeyFrame frame = node.Value;
                if (timeStamp < frame.Timestamp) {
                    this.keyFrames.AddBefore(node, keyFrame);
                }
                else if (timeStamp == frame.Timestamp) {
                    this.keyFrames.AddAfter(node, keyFrame);
                }
                else {
                    continue;
                }

                return;
            }

            this.keyFrames.AddLast(keyFrame);
        }

        public bool RemoveKeyFrame(KeyFrame frame) {
            for (LinkedListNode<KeyFrame> node = this.keyFrames.First; node != null; node = node.Next) {
                if (node.Value.Equals(frame)) {
                    this.keyFrames.Remove(node);
                    return true;
                }
            }

            return false;
        }

        public void WriteToRBE(RBEDictionary data) {

        }

        public void ReadFromRBE(RBEDictionary data) {

        }
    }
}