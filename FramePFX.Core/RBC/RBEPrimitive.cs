using System.IO;

namespace FramePFX.Core.RBC {
    public class RBEInt8 : RBEBase {
        public override int TypeId => 3;

        public byte Value { get; set; }

        public RBEInt8() {

        }

        public RBEInt8(byte value) {
            this.Value = value;
        }

        public override void Read(BinaryReader reader) {
            this.Value = reader.ReadByte();
        }

        public override void Write(BinaryWriter writer) {
            writer.Write(this.Value);
        }
    }

    public class RBEInt16 : RBEBase {
        public override int TypeId => 4;

        public short Value { get; set; }

        public RBEInt16() {

        }

        public RBEInt16(short value) {
            this.Value = value;
        }

        public override void Read(BinaryReader reader) {
            this.Value = reader.ReadInt16();
        }

        public override void Write(BinaryWriter writer) {
            writer.Write(this.Value);
        }
    }

    public class RBEInt32 : RBEBase {
        public override int TypeId => 5;

        public int Value { get; set; }

        public RBEInt32() {

        }

        public RBEInt32(int value) {
            this.Value = value;
        }

        public override void Read(BinaryReader reader) {
            this.Value = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer) {
            writer.Write(this.Value);
        }
    }

    public class RBEInt64 : RBEBase {
        public override int TypeId => 6;

        public long Value { get; set; }

        public RBEInt64() {

        }

        public RBEInt64(long value) {
            this.Value = value;
        }

        public override void Read(BinaryReader reader) {
            this.Value = reader.ReadInt64();
        }

        public override void Write(BinaryWriter writer) {
            writer.Write(this.Value);
        }
    }

    public class RBEFloat : RBEBase {
        public override int TypeId => 7;

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
    }

    public class RBEDouble : RBEBase {
        public override int TypeId => 8;

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
    }
}