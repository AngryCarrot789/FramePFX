// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

namespace FramePFX.Utils.BTE;

public enum LengthReadStrategy {
    /// <summary>
    /// Appends a byte, short, int or long before the data to indicate the length.
    /// This is fixed per BTE type and does not change, and is therefore the easiest to implement
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
public class BTEByteArray : BinaryTreeElement {
    public byte[]? Array { get; set; }

    public bool IsEmpty {
        get => this.Array == null || this.Array.Length == 0;
    }

    public override BTEType Type => BTEType.ByteArray;

    public BTEByteArray() {
    }

    public BTEByteArray(byte[]? array) {
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

    public override BinaryTreeElement Clone() => this.CloneCore();

    public BTEByteArray CloneCore() {
        return new BTEByteArray(this.Array.CloneArrayUnsafe());
    }
}

public class BTEShortArray : BinaryTreeElement {
    public short[]? Array { get; set; }

    public bool IsEmpty {
        get => this.Array == null || this.Array.Length == 0;
    }

    public override BTEType Type => BTEType.ShortArray;

    public BTEShortArray() {
    }

    public BTEShortArray(short[]? array) {
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

    public override BinaryTreeElement Clone() => this.CloneCore();

    public BTEShortArray CloneCore() {
        return new BTEShortArray(this.Array.CloneArrayUnsafe());
    }
}

/// <summary>
/// Used to store an array of integers (signed)
/// </summary>
public class BTEIntArray : BinaryTreeElement {
    public int[]? Array { get; set; }

    public bool IsEmpty {
        get => this.Array == null || this.Array.Length == 0;
    }

    public override BTEType Type => BTEType.IntArray;

    public BTEIntArray() {
    }

    public BTEIntArray(int[]? array) {
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

    public override BinaryTreeElement Clone() => this.CloneCore();

    public BTEIntArray CloneCore() {
        return new BTEIntArray(this.Array.CloneArrayUnsafe());
    }
}

public class BTELongArray : BinaryTreeElement {
    public long[]? Array { get; set; }

    public bool IsEmpty {
        get => this.Array == null || this.Array.Length == 0;
    }

    public override BTEType Type => BTEType.LongArray;

    public BTELongArray() {
    }

    public BTELongArray(long[]? array) {
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

    public override BinaryTreeElement Clone() => this.CloneCore();

    public BTELongArray CloneCore() {
        return new BTELongArray(this.Array.CloneArrayUnsafe());
    }
}

public class BTEFloatArray : BinaryTreeElement {
    public float[]? Array { get; set; }

    public bool IsEmpty {
        get => this.Array == null || this.Array.Length == 0;
    }

    public override BTEType Type => BTEType.FloatArray;

    public BTEFloatArray() {
    }

    public BTEFloatArray(float[]? array) {
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

    public override BinaryTreeElement Clone() => this.CloneCore();

    public BTEFloatArray CloneCore() {
        return new BTEFloatArray(this.Array.CloneArrayUnsafe());
    }
}

public class BTEDoubleArray : BinaryTreeElement {
    public double[]? Array { get; set; }

    public bool IsEmpty {
        get => this.Array == null || this.Array.Length == 0;
    }

    public override BTEType Type => BTEType.DoubleArray;

    public BTEDoubleArray() {
    }

    public BTEDoubleArray(double[]? array) {
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

    public override BinaryTreeElement Clone() => this.CloneCore();

    public BTEDoubleArray CloneCore() {
        return new BTEDoubleArray(this.Array.CloneArrayUnsafe());
    }
}

public class BTEStringArray : BinaryTreeElement {
    public override BTEType Type => BTEType.StringArray;

    public string[]? Array { get; set; }

    public BTEStringArray() {
    }

    public BTEStringArray(string[]? array) {
        this.Array = array;
    }

    protected override void Read(BinaryReader reader) {
        int length = reader.ReadInt32();
        string[] array = this.Array = new string[length];
        for (int i = 0; i < length; i++) {
            array[i] = BTEString.ReadString(reader);
        }
    }

    protected override void Write(BinaryWriter writer) {
        if (this.Array != null) {
            writer.Write(this.Array.Length);
            foreach (string value in this.Array) {
                BTEString.WriteString(writer, value);
            }
        }
        else {
            writer.Write(0);
        }
    }

    public override BinaryTreeElement Clone() => this.CloneCore();
    public BTEStringArray CloneCore() => new BTEStringArray(this.Array.CloneArrayMax());
}

public class BTEStructArray : BinaryTreeElement {
    private byte[] data;

    public override BTEType Type => BTEType.StructArray;

    public BTEStructArray() {
    }

    public static BTEStructArray ForValues<T>(T[] value) where T : unmanaged {
        BTEStructArray bte = new BTEStructArray();
        bte.SetValues(value);
        return bte;
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
                values[i] = BinaryUtils.ReadStruct<T>(array, offset);
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
                values[i] = BinaryUtils.ReadStruct<T>(array, offset);
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
                BinaryUtils.WriteStruct(values[i], array, offset);
            }
        }
    }

    public override BinaryTreeElement Clone() => this.CloneCore();

    public BTEStructArray CloneCore() {
        return new BTEStructArray { data = this.data.CloneArrayUnsafe() };
    }
}