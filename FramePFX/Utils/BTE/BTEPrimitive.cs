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

using System.Text;

namespace FramePFX.Utils.BTE;

public class BTEByte : BinaryTreeElement {
    public override BTEType Type => BTEType.Byte;

    public byte Value { get; set; }

    public BTEByte() {
    }

    public BTEByte(byte value) {
        this.Value = value;
    }

    protected override void Read(BinaryReader reader) {
        this.Value = reader.ReadByte();
    }

    protected override void Write(BinaryWriter writer) {
        writer.Write(this.Value);
    }

    public override BinaryTreeElement Clone() => this.CloneCore();
    public BTEByte CloneCore() => new BTEByte(this.Value);
}

public class BTEShort : BinaryTreeElement {
    public override BTEType Type => BTEType.Short;

    public short Value { get; set; }

    public BTEShort() {
    }

    public BTEShort(short value) {
        this.Value = value;
    }

    protected override void Read(BinaryReader reader) {
        this.Value = reader.ReadInt16();
    }

    protected override void Write(BinaryWriter writer) {
        writer.Write(this.Value);
    }

    public override BinaryTreeElement Clone() => this.CloneCore();
    public BTEShort CloneCore() => new BTEShort(this.Value);
}

public class BTEInt : BinaryTreeElement {
    public override BTEType Type => BTEType.Int;

    public int Value { get; set; }

    public BTEInt() {
    }

    public BTEInt(int value) {
        this.Value = value;
    }

    protected override void Read(BinaryReader reader) {
        this.Value = reader.ReadInt32();
    }

    protected override void Write(BinaryWriter writer) {
        writer.Write(this.Value);
    }

    public override BinaryTreeElement Clone() => this.CloneCore();
    public BTEInt CloneCore() => new BTEInt(this.Value);
}

public class BTELong : BinaryTreeElement {
    public override BTEType Type => BTEType.Long;

    public long Value { get; set; }

    public BTELong() {
    }

    public BTELong(long value) {
        this.Value = value;
    }

    protected override void Read(BinaryReader reader) {
        this.Value = reader.ReadInt64();
    }

    protected override void Write(BinaryWriter writer) {
        writer.Write(this.Value);
    }

    public override BinaryTreeElement Clone() => this.CloneCore();
    public BTELong CloneCore() => new BTELong(this.Value);
}

public class BTEFloat : BinaryTreeElement {
    public override BTEType Type => BTEType.Float;

    public float Value { get; set; }

    public BTEFloat() {
    }

    public BTEFloat(float value) {
        this.Value = value;
    }

    protected override void Read(BinaryReader reader) {
        this.Value = reader.ReadSingle();
    }

    protected override void Write(BinaryWriter writer) {
        writer.Write(this.Value);
    }

    public override BinaryTreeElement Clone() => this.CloneCore();
    public BTEFloat CloneCore() => new BTEFloat(this.Value);
}

public class BTEDouble : BinaryTreeElement {
    public override BTEType Type => BTEType.Double;

    public double Value { get; set; }

    public BTEDouble() {
    }

    public BTEDouble(double value) {
        this.Value = value;
    }

    protected override void Read(BinaryReader reader) {
        this.Value = reader.ReadDouble();
    }

    protected override void Write(BinaryWriter writer) {
        writer.Write(this.Value);
    }

    public override BinaryTreeElement Clone() => this.CloneCore();
    public BTEDouble CloneCore() => new BTEDouble(this.Value);
}

/// <summary>
/// An BTE element that stores a string. Max string length is an unsigned short (<see cref="ushort.MaxValue"/>)
/// </summary>
public class BTEString : BinaryTreeElement {
    public const int MaxValueLength = ushort.MaxValue;
    private string? value;

    public override BTEType Type => BTEType.String;

    public string? Value {
        get => this.value;
        set {
            if (value != null && value.Length > MaxValueLength) {
                throw new Exception("Value length exceeds the maximum value of " + MaxValueLength);
            }

            this.value = value;
        }
    }

    public BTEString() {
    }

    public BTEString(string? value) {
        this.Value = value;
    }

    public static string ClampLength(string? input) {
        if (input != null && input.Length > MaxValueLength)
            return input.Substring(0, MaxValueLength);
        return input;
    }

    public static void CreateStringList(BTEList list, string input) {
        for (int i = 0, length = input.Length; i < length; i += MaxValueLength) {
            list.Add(new BTEString(input.Substring(i, Math.Min(MaxValueLength, length - i))));
        }
    }

    public static BTEList CreateStringList(string input) {
        BTEList list = new BTEList(new List<BinaryTreeElement>(input.Length / MaxValueLength + 1));
        CreateStringList(list, input);
        return list;
    }

    public static string ReadFromStringList(BTEList list) {
        IEnumerable<BTEString> enumerable = list.Cast<BTEString>();
        StringBuilder sb = new StringBuilder(list.List.Count * MaxValueLength);
        foreach (BTEString bte in enumerable)
            sb.Append(bte.value);
        return sb.ToString();
    }

    /// <summary>
    /// Reads a ushort (as a length prefix) and then reads that many chars as a string
    /// </summary>
    /// <param name="reader"></param>
    /// <returns>A string with more than 0 character, or null. This function does not return an empty string</returns>
    public static string? ReadString(BinaryReader reader) {
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
    public static void WriteString(BinaryWriter writer, string? text) {
        if (string.IsNullOrEmpty(text)) {
            writer.Write((ushort) 0);
        }
        else {
            writer.Write((ushort) text.Length);
            writer.Write(text.ToCharArray(0, Math.Min(text.Length, ushort.MaxValue)));
        }
    }

    protected override void Read(BinaryReader reader) {
        this.value = ReadString(reader);
    }

    protected override void Write(BinaryWriter writer) {
        WriteString(writer, this.value);
    }

    public override BinaryTreeElement Clone() => this.CloneCore();
    public BTEString CloneCore() => new BTEString(this.value);
}