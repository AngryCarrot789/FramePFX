using System;
using System.IO;
using FramePFX.Utils;

namespace FramePFX.RBC {
    public enum LengthReadStrategy {
        /// <summary>
        /// Appends a byte, short, int or long before the data to indicate the length.
        /// This is fixed per RBE type and does not change, and is therefore the easiest to implement
        /// </summary>
        LazyLength,

        /// <summary>
        /// Uses the first two bits to determine how many additional bytes to read to discover the length. This is used to
        /// compress the data as much as possible, but is harder to implement and typically slower to read/write, and can
        /// sometimes result in more data being used overall (255 >> 2 == 63, meaning if you need 64 values, you need 2 bytes to store 64)
        /// </summary>
        SegmentedLength
    }

    /// <summary>
    /// Used to store an array of bytes (unsigned)
    /// </summary>
    public class RBEByteArray : RBEBase {
        public byte[] Array { get; set; }

        public bool IsEmpty {
            get => this.Array == null || this.Array.Length == 0;
        }

        public override RBEType Type => RBEType.ByteArray;

        public RBEByteArray() {
        }

        public RBEByteArray(byte[] array) {
            this.Array = array;
        }

        protected override void Read(BinaryReader reader) {
            this.Array = new byte[reader.ReadInt32()];
            reader.Read(this.Array, 0, this.Array.Length);
        }

        protected override void Write(BinaryWriter writer) {
            if (this.Array != null) {
                writer.Write(this.Array.Length);
                writer.Write(this.Array);
            }
            else {
                writer.Write(0);
            }
        }

        public override RBEBase Clone() => this.CloneCore();

        public RBEByteArray CloneCore() {
            return new RBEByteArray(Arrays.CloneArrayUnsafe(this.Array));
        }
    }

    public class RBEShortArray : RBEBase {
        public short[] Array { get; set; }

        public bool IsEmpty {
            get => this.Array == null || this.Array.Length == 0;
        }

        public override RBEType Type => RBEType.ShortArray;

        public RBEShortArray() {
        }

        public RBEShortArray(short[] array) {
            this.Array = array;
        }

        protected override void Read(BinaryReader reader) {
            int length = reader.ReadInt32();
            short[] array = this.Array = new short[length];
            for (int i = 0; i < length; i++) {
                array[i] = reader.ReadInt16();
            }
        }

        protected override void Write(BinaryWriter writer) {
            if (this.Array != null) {
                writer.Write(this.Array.Length);
                foreach (short value in this.Array) {
                    writer.Write(value);
                }
            }
            else {
                writer.Write(0);
            }
        }

        public override RBEBase Clone() => this.CloneCore();

        public RBEShortArray CloneCore() {
            return new RBEShortArray(Arrays.CloneArrayUnsafe(this.Array));
        }
    }

    /// <summary>
    /// Used to store an array of integers (signed)
    /// </summary>
    public class RBEIntArray : RBEBase {
        public int[] Array { get; set; }

        public bool IsEmpty {
            get => this.Array == null || this.Array.Length == 0;
        }

        public override RBEType Type => RBEType.IntArray;

        public RBEIntArray() {
        }

        public RBEIntArray(int[] array) {
            this.Array = array;
        }

        protected override void Read(BinaryReader reader) {
            int length = reader.ReadInt32();
            int[] array = this.Array = new int[length];
            for (int i = 0; i < length; i++) {
                array[i] = reader.ReadInt32();
            }

            // optimise for reading long?
            // for (int i = 0, end = length >> 1; i <= end; i++) { }
        }

        protected override void Write(BinaryWriter writer) {
            if (this.Array != null) {
                writer.Write(this.Array.Length);
                foreach (int value in this.Array) {
                    writer.Write(value);
                }
            }
            else {
                writer.Write(0);
            }
        }

        public override RBEBase Clone() => this.CloneCore();

        public RBEIntArray CloneCore() {
            return new RBEIntArray(Arrays.CloneArrayUnsafe(this.Array));
        }
    }

    public class RBELongArray : RBEBase {
        public long[] Array { get; set; }

        public bool IsEmpty {
            get => this.Array == null || this.Array.Length == 0;
        }

        public override RBEType Type => RBEType.LongArray;

        public RBELongArray() {
        }

        public RBELongArray(long[] array) {
            this.Array = array;
        }

        protected override void Read(BinaryReader reader) {
            int length = reader.ReadInt32();
            long[] array = this.Array = new long[length];
            for (int i = 0; i < length; i++) {
                array[i] = reader.ReadInt64();
            }

            // optimise for reading long?
            // for (int i = 0, end = length >> 1; i <= end; i++) { }
        }

        protected override void Write(BinaryWriter writer) {
            if (this.Array != null) {
                writer.Write(this.Array.Length);
                foreach (long value in this.Array) {
                    writer.Write(value);
                }
            }
            else {
                writer.Write(0);
            }
        }

        public override RBEBase Clone() => this.CloneCore();

        public RBELongArray CloneCore() {
            return new RBELongArray(Arrays.CloneArrayUnsafe(this.Array));
        }
    }

    public class RBEFloatArray : RBEBase {
        public float[] Array { get; set; }

        public bool IsEmpty {
            get => this.Array == null || this.Array.Length == 0;
        }

        public override RBEType Type => RBEType.FloatArray;

        public RBEFloatArray() {
        }

        public RBEFloatArray(float[] array) {
            this.Array = array;
        }

        protected override void Read(BinaryReader reader) {
            int length = reader.ReadInt32();
            float[] array = this.Array = new float[length];
            for (int i = 0; i < length; i++) {
                array[i] = reader.ReadSingle();
            }
        }

        protected override void Write(BinaryWriter writer) {
            if (this.Array != null) {
                writer.Write(this.Array.Length);
                foreach (float value in this.Array) {
                    writer.Write(value);
                }
            }
            else {
                writer.Write(0);
            }
        }

        public override RBEBase Clone() => this.CloneCore();

        public RBEFloatArray CloneCore() {
            return new RBEFloatArray(Arrays.CloneArrayUnsafe(this.Array));
        }
    }

    public class RBEDoubleArray : RBEBase {
        public double[] Array { get; set; }

        public bool IsEmpty {
            get => this.Array == null || this.Array.Length == 0;
        }

        public override RBEType Type => RBEType.DoubleArray;

        public RBEDoubleArray() {
        }

        public RBEDoubleArray(double[] array) {
            this.Array = array;
        }

        protected override void Read(BinaryReader reader) {
            int length = reader.ReadInt32();
            double[] array = this.Array = new double[length];
            for (int i = 0; i < length; i++) {
                array[i] = reader.ReadDouble();
            }
        }

        protected override void Write(BinaryWriter writer) {
            if (this.Array != null) {
                writer.Write(this.Array.Length);
                foreach (double value in this.Array) {
                    writer.Write(value);
                }
            }
            else {
                writer.Write(0);
            }
        }

        public override RBEBase Clone() => this.CloneCore();

        public RBEDoubleArray CloneCore() {
            return new RBEDoubleArray(Arrays.CloneArrayUnsafe(this.Array));
        }
    }

    public class RBEStringArray : RBEBase {
        public override RBEType Type => RBEType.StringArray;

        public string[] Array { get; set; }

        public RBEStringArray() {
        }

        public RBEStringArray(string[] array) {
            this.Array = array;
        }

        protected override void Read(BinaryReader reader) {
            int length = reader.ReadInt32();
            string[] array = this.Array = new string[length];
            for (int i = 0; i < length; i++) {
                array[i] = RBEString.ReadString(reader);
            }
        }

        protected override void Write(BinaryWriter writer) {
            if (this.Array != null) {
                writer.Write(this.Array.Length);
                foreach (string value in this.Array) {
                    RBEString.WriteString(writer, value);
                }
            }
            else {
                writer.Write(0);
            }
        }

        public override RBEBase Clone() => this.CloneCore();
        public RBEStringArray CloneCore() => new RBEStringArray(Arrays.CloneArray(this.Array));
    }

    public class RBEStructArray : RBEBase {
        private byte[] data;

        public override RBEType Type => RBEType.StructArray;

        public RBEStructArray() {
        }

        public static RBEStructArray ForValues<T>(T[] value) where T : unmanaged {
            RBEStructArray rbe = new RBEStructArray();
            rbe.SetValues(value);
            return rbe;
        }

        protected override void Read(BinaryReader reader) {
            int length = reader.ReadInt32();
            this.data = new byte[length];
            if (reader.Read(this.data, 0, length) != length) {
                throw new IOException("Failed to read " + length + " bytes");
            }
        }

        protected override void Write(BinaryWriter writer) {
            if (this.data == null) {
                throw new InvalidOperationException("Array has not been set yet");
            }

            // could possibly optimise this; use 2 bits to indicate the size type:
            // byte,short,int,long (long is a bit excessive though)
            // then bitshift the actual size by 2, cast to size type, and write?
            writer.Write(this.data.Length);
            writer.Write(this.data);
        }

        public T[] GetValues<T>() where T : unmanaged {
            unsafe {
                byte[] array = this.data;
                if (array == null) {
                    throw new Exception("Binary data has not been read yet");
                }

                int size = sizeof(T);
                if ((array.Length % size) != 0) {
                    throw new Exception($"Binary data size is inconsistent with the struct size (binary({array.Length}) % struct({size})) != 0");
                }

                int len = array.Length / size;
                T[] values = new T[len];
                for (int i = 0, offset = 0; i < len; i++, offset += size) {
                    values[i] = BinaryUtils.ReadStruct<T>(array, offset, size);
                }

                return values;
            }
        }

        public bool TryGetValues<T>(out T[] values) where T : unmanaged {
            byte[] array = this.data;
            unsafe {
                int size;
                if (array == null || ((size = sizeof(T)) % size) != 0) {
                    values = null;
                    return false;
                }

                int len = array.Length / size;
                values = new T[len];
                for (int i = 0, offset = 0; i < len; i++, offset += size) {
                    values[i] = BinaryUtils.ReadStruct<T>(array, offset, size);
                }

                return true;
            }
        }

        public void SetValues<T>(T[] values) where T : unmanaged {
            unsafe {
                int size = sizeof(T);
                int length = values.Length;
                byte[] array = this.data = new byte[size * length];
                for (int i = 0, offset = 0; i < length; i++, offset += size) {
                    BinaryUtils.WriteStruct(values[i], array, offset, size);
                }
            }
        }

        public override RBEBase Clone() => this.CloneCore();

        public RBEStructArray CloneCore() {
            return new RBEStructArray {data = Arrays.CloneArrayUnsafe(this.data)};
        }
    }
}