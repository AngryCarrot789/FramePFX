using FramePFX.Core.RBC;
using OpenTK;

namespace FramePFX.Core.Automation.Keyframe {
    public abstract class KeyFrame : IRBESerialisable {
        public long Timestamp { get; set; }

        public abstract AutomationDataType DataType { get; }

        protected KeyFrame(long timestamp) {
            this.Timestamp = timestamp;
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

        public KeyFrameDouble(long timestamp) : base(timestamp) {

        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetDouble(nameof(this.Value), this.Value);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Value = data.GetDouble(nameof(this.Value));
        }
    }

    public class KeyFrameLong : KeyFrame {
        public long Value { get; set; }

        public override AutomationDataType DataType => AutomationDataType.Long;

        public KeyFrameLong(long timestamp) : base(timestamp) {

        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetLong(nameof(this.Value), this.Value);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Value = data.GetLong(nameof(this.Value));
        }
    }

    public class KeyFrameBoolean : KeyFrame {
        public bool Value { get; set; }

        public override AutomationDataType DataType => AutomationDataType.Boolean;

        public KeyFrameBoolean(long timestamp) : base(timestamp) {

        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetBool(nameof(this.Value), this.Value);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Value = data.GetBool(nameof(this.Value));
        }
    }

    public class KeyFrameVector2 : KeyFrame {
        public Vector2 Value { get; set; }

        public override AutomationDataType DataType => AutomationDataType.Vector2;

        public KeyFrameVector2(long timestamp) : base(timestamp) {

        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetStruct(nameof(this.Value), this.Value);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Value = data.GetStruct<Vector2>(nameof(this.Value));
        }
    }
}