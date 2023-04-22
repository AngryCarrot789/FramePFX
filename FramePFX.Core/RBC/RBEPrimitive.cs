using System.IO;

namespace FramePFX.Core.RBC {
    public class RBEByte : RBEBase {
        public override RBEType Type => RBEType.Byte;

        public byte Value { get; set; }

        public RBEByte() {

        }

        public RBEByte(byte value) {
            this.Value = value;
        }

        public override void Read(BinaryReader reader) {
            this.Value = reader.ReadByte();
        }

        public override void Write(BinaryWriter writer) {
            writer.Write(this.Value);
        }

        public override RBEBase CloneCore() => this.Clone();
        public RBEByte Clone() => new RBEByte(this.Value);
    }

    public class RBEShort : RBEBase {
        public override RBEType Type => RBEType.Short;

        public short Value { get; set; }

        public RBEShort() {

        }

        public RBEShort(short value) {
            this.Value = value;
        }

        public override void Read(BinaryReader reader) {
            this.Value = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer) {
            writer.Write(this.Value);
        }

        public override RBEBase CloneCore() => this.Clone();
        public RBEShort Clone() => new RBEShort(this.Value);
    }

    public class RBEInt : RBEBase {
        public override RBEType Type => RBEType.Int;

        public int Value { get; set; }

        public RBEInt() {

        }

        public RBEInt(int value) {
            this.Value = value;
        }

        public override void Read(BinaryReader reader) {
            this.Value = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer) {
            writer.Write(this.Value);
        }

        public override RBEBase CloneCore() => this.Clone();
        public RBEInt Clone() => new RBEInt(this.Value);
    }

    public class RBELong : RBEBase {
        public override RBEType Type => RBEType.Long;

        public long Value { get; set; }

        public RBELong() {

        }

        public RBELong(long value) {
            this.Value = value;
        }

        public override void Read(BinaryReader reader) {
            this.Value = reader.ReadInt64();
        }

        public override void Write(BinaryWriter writer) {
            writer.Write(this.Value);
        }

        public override RBEBase CloneCore() => this.Clone();
        public RBELong Clone() => new RBELong(this.Value);
    }

    public class RBEFloat : RBEBase {
        public override RBEType Type => RBEType.Float;

        public float Value { get; set; }

        public RBEFloat() {

        }

        public RBEFloat(float value) {
            this.Value = value;
        }

        public override void Read(BinaryReader reader) {
            this.Value = reader.ReadSingle();
        }

        public override void Write(BinaryWriter writer) {
            writer.Write(this.Value);
        }

        public override RBEBase CloneCore() => this.Clone();
        public RBEFloat Clone() => new RBEFloat(this.Value);
    }

    public class RBEDouble : RBEBase {
        public override RBEType Type => RBEType.Double;

        public double Value { get; set; }

        public RBEDouble() {

        }

        public RBEDouble(double value) {
            this.Value = value;
        }

        public override void Read(BinaryReader reader) {
            this.Value = reader.ReadDouble();
        }

        public override void Write(BinaryWriter writer) {
            writer.Write(this.Value);
        }

        public override RBEBase CloneCore() => this.Clone();
        public RBEDouble Clone() => new RBEDouble(this.Value);
    }
}