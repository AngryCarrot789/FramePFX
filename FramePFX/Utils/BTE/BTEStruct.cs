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

using System.Runtime.CompilerServices;

namespace FramePFX.Utils.BTE;

/// <summary>
/// Used to store unmanaged structs in the little-endian format. Struct can have a max size of 65535 (<see cref="ushort.MaxValue"/>) bytes
/// <para>
/// Only unmanaged structs can be stored. These are just simple structs, e.g. int, long, etc, or any custom struct that consists of those. A struct
/// that contains a reference type as a field/property is not unmanaged and cannot be stored (maybe apart from strings? not sure)
/// </para>
/// </summary>
public class BTEStruct : BinaryTreeElement {
    private byte[] data;

    public override BTEType Type => BTEType.Struct;

    public BTEStruct() {
    }

    public static BTEStruct ForValue<T>(in T value) where T : unmanaged {
        BTEStruct bte = new BTEStruct();
        bte.SetValue(value);
        return bte;
    }

    protected override void Read(BinaryReader reader) {
        int length = reader.ReadUInt16();
        this.data = new byte[length];
        if (reader.Read(this.data, 0, length) != length) {
            throw new IOException("Failed to read " + length + " bytes");
        }
    }

    protected override void Write(BinaryWriter writer) {
        if (this.data == null) {
            throw new InvalidOperationException("Array has not been set yet");
        }

        // no one would ever have a struct whose size is greater than 65535
        writer.Write((ushort) this.data.Length);
        writer.Write(this.data);
    }

    public T GetValue<T>() where T : unmanaged {
        byte[] array = this.data;
        if (array == null) {
            throw new Exception("Binary data has not been read yet");
        }

        int size = Unsafe.SizeOf<T>();
        if (array.Length != size) {
            throw new Exception($"Binary data size does not match struct size (binary({array.Length}) != struct({size}) for struct {typeof(T)})");
        }

        return BinaryUtils.ReadStruct<T>(array, 0);
    }

    public bool TryGetValue<T>(out T value) where T : unmanaged {
        int size;
        if (this.data == null || this.data.Length != (size = Unsafe.SizeOf<T>())) {
            value = default;
            return false;
        }

        value = BinaryUtils.ReadStruct<T>(this.data, 0);
        return true;
    }

    public void SetValue<T>(in T value) where T : unmanaged {
        int size = Unsafe.SizeOf<T>();
        if (size > ushort.MaxValue) {
            throw new Exception("Value's size is too large: " + size);
        }

        this.data = new byte[size];
        BinaryUtils.WriteStruct(value, this.data, 0);
    }

    public override BinaryTreeElement Clone() => this.CloneCore();

    public BTEStruct CloneCore() {
        byte[] src = this.data, dest = null;
        if (src != null) {
            dest = new byte[src.Length];
            Unsafe.CopyBlock(ref dest[0], ref src[0], (uint) dest.Length);
        }

        return new BTEStruct { data = dest };
    }
}