using System;
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

        protected override void Read(BinaryReader reader) {
            this.Value = reader.ReadByte();
        }

        protected override void Write(BinaryWriter writer) {
            writer.Write(this.Value);
        }

        public override RBEBase Clone() => this.CloneCore();
        public RBEByte CloneCore() => new RBEByte(this.Value);
    }

    public class RBEShort : RBEBase {
        public override RBEType Type => RBEType.Short;

        public short Value { get; set; }

        public RBEShort() {
        }

        public RBEShort(short value) {
            this.Value = value;
        }

        protected override void Read(BinaryReader reader) {
            this.Value = reader.ReadInt16();
        }

        protected override void Write(BinaryWriter writer) {
            writer.Write(this.Value);
        }

        public override RBEBase Clone() => this.CloneCore();
        public RBEShort CloneCore() => new RBEShort(this.Value);
    }

    public class RBEInt : RBEBase {
        public override RBEType Type => RBEType.Int;

        public int Value { get; set; }

        public RBEInt() {
        }

        public RBEInt(int value) {
            this.Value = value;
        }

        protected override void Read(BinaryReader reader) {
            this.Value = reader.ReadInt32();
        }

        protected override void Write(BinaryWriter writer) {
            writer.Write(this.Value);
        }

        public override RBEBase Clone() => this.CloneCore();
        public RBEInt CloneCore() => new RBEInt(this.Value);
    }

    public class RBELong : RBEBase {
        public override RBEType Type => RBEType.Long;

        public long Value { get; set; }

        public RBELong() {
        }

        public RBELong(long value) {
            this.Value = value;
        }

        protected override void Read(BinaryReader reader) {
            this.Value = reader.ReadInt64();
        }

        protected override void Write(BinaryWriter writer) {
            writer.Write(this.Value);
        }

        public override RBEBase Clone() => this.CloneCore();
        public RBELong CloneCore() => new RBELong(this.Value);
    }

    public class RBEFloat : RBEBase {
        public override RBEType Type => RBEType.Float;

        public float Value { get; set; }

        public RBEFloat() {
        }

        public RBEFloat(float value) {
            this.Value = value;
        }

        protected override void Read(BinaryReader reader) {
            this.Value = reader.ReadSingle();
        }

        protected override void Write(BinaryWriter writer) {
            writer.Write(this.Value);
        }

        public override RBEBase Clone() => this.CloneCore();
        public RBEFloat CloneCore() => new RBEFloat(this.Value);
    }

    public class RBEDouble : RBEBase {
        public override RBEType Type => RBEType.Double;

        public double Value { get; set; }

        public RBEDouble() {
        }

        public RBEDouble(double value) {
            this.Value = value;
        }

        protected override void Read(BinaryReader reader) {
            this.Value = reader.ReadDouble();
        }

        protected override void Write(BinaryWriter writer) {
            writer.Write(this.Value);
        }

        public override RBEBase Clone() => this.CloneCore();
        public RBEDouble CloneCore() => new RBEDouble(this.Value);
    }

    /// <summary>
    /// An RBE element that stores a string. Max string length is an unsigned short (<see cref="ushort.MaxValue"/>)
    /// </summary>
    public class RBEString : RBEBase {
        public override RBEType Type => RBEType.String;

        public string Value { get; set; }

        public RBEString() {
        }

        public RBEString(string value) {
            this.Value = value;
        }

        /// <summary>
        /// Reads a ushort (as a length prefix) and then reads that many chars as a string
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>A string with more than 0 character, or null. This function does not return an empty string</returns>
        public static string ReadString(BinaryReader reader) {
            int length = reader.ReadUInt16();
            if (length < 1) {
                return null;
            }
            else {
                char[] chars = reader.ReadChars(length);
                return new string(chars);
            }
        }

        /// <summary>
        /// Writes a ushort (as a length prefix) and then the chars of the string. If the string is too long, the excess is not written
        /// </summary>
        public static void WriteString(BinaryWriter writer, string text) {
            if (string.IsNullOrEmpty(text)) {
                writer.Write((ushort) 0);
            }
            else {
                writer.Write((ushort) text.Length);
                writer.Write(text.ToCharArray(0, Math.Min(text.Length, ushort.MaxValue)));
            }
        }

        protected override void Read(BinaryReader reader) {
            this.Value = ReadString(reader);
        }

        protected override void Write(BinaryWriter writer) {
            WriteString(writer, this.Value);
        }

        public override RBEBase Clone() => this.CloneCore();
        public RBEString CloneCore() => new RBEString(this.Value);
    }
}