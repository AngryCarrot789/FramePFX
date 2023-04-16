using System;
using System.IO;
using System.Runtime.ExceptionServices;

namespace FramePFX.Core.RBC {
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

        public override void Read(BinaryReader reader) {
            this.Array = new byte[reader.ReadInt32()];
            reader.Read(this.Array, 0, this.Array.Length);
        }

        public override void Write(BinaryWriter writer) {
            if (this.Array != null) {
                writer.Write(this.Array.Length);
                writer.Write(this.Array);
            }
            else {
                writer.Write(0);
            }
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

        public override void Read(BinaryReader reader) {
            int length = reader.ReadInt32();
            short[] array = this.Array = new short[length];
            for (int i = 0; i < length; i++) {
                array[i] = reader.ReadInt16();
            }
        }

        public override void Write(BinaryWriter writer) {
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

        public override void Read(BinaryReader reader) {
            int length = reader.ReadInt32();
            int[] array = this.Array = new int[length];
            for (int i = 0; i < length; i++) {
                array[i] = reader.ReadInt32();
            }

            // optimise for reading long?
            // for (int i = 0, end = length >> 1; i <= end; i++) { }
        }

        public override void Write(BinaryWriter writer) {
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

        public override void Read(BinaryReader reader) {
            int length = reader.ReadInt32();
            long[] array = this.Array = new long[length];
            for (int i = 0; i < length; i++) {
                array[i] = reader.ReadInt64();
            }

            // optimise for reading long?
            // for (int i = 0, end = length >> 1; i <= end; i++) { }
        }

        public override void Write(BinaryWriter writer) {
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

        public override void Read(BinaryReader reader) {
            int length = reader.ReadInt32();
            float[] array = this.Array = new float[length];
            for (int i = 0; i < length; i++) {
                array[i] = reader.ReadSingle();
            }
        }

        public override void Write(BinaryWriter writer) {
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

        public override void Read(BinaryReader reader) {
            int length = reader.ReadInt32();
            double[] array = this.Array = new double[length];
            for (int i = 0; i < length; i++) {
                array[i] = reader.ReadDouble();
            }
        }

        public override void Write(BinaryWriter writer) {
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
    }

    public class RBEStructArray : RBEBase {
        private byte[] data;

        public override RBEType Type => RBEType.StructArray;

        public RBEStructArray() {

        }

        public override void Read(BinaryReader reader) {
            int length = reader.ReadInt32();
            this.data = new byte[length];
            if (reader.Read(this.data, 0, length) != length) {
                throw new IOException("Failed to read " + length + " bytes");
            }
        }

        public override void Write(BinaryWriter writer) {
            if (this.data == null) {
                throw new InvalidOperationException("Array has not been set yet");
            }

            writer.Write(this.data.Length);
            writer.Write(this.data);
        }

        public T[] GetValues<T>() where T : unmanaged {
            unsafe {
                if (this.data == null) {
                    throw new Exception("Binary data has not been read yet");
                }

                int size = sizeof(T);
                if ((this.data.Length % size) != 0) {
                    throw new Exception($"Binary data size is inconsistent with the struct size ({this.data.Length} % {size}) != 0");
                }

                T[] array = new T[this.data.Length / size];
                for (int i = 0, j = 0; i < array.Length; i++, j += size) {
                    T value = new T();
                    BinaryUtils.CopyArray(this.data, j, (byte*) &value, 0, size);
                    array[i] = value;
                }

                return array;
            }
        }

        public void SetValues<T>(T[] values) where T : unmanaged {
            unsafe {
                int size = sizeof(T), length = values.Length;
                this.data = new byte[size * length];
                for (int i = 0, j = 0; i < length; i++, j += size) {
                    T value = values[i];
                    BinaryUtils.WriteArray((byte*) &value, 0, this.data, j, size);
                }
            }
        }
    }
}