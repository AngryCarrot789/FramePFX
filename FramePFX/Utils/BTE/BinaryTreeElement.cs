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

/// <summary>
/// The base class for a binary tree. A binary tree is a tree of objects that represent different pieces
/// of data (primitives, structs, lists and dictionaries). These are all serialisable and deserialisable
/// to and from a file (in pure binary data). 
/// </summary>
public abstract class BinaryTreeElement {
    private static readonly Dictionary<Type, BTEType> TypeToIdTable;

    /// <summary>
    /// This element's type
    /// </summary>
    public abstract BTEType Type { get; }

    static BinaryTreeElement() {
        TypeToIdTable = new Dictionary<Type, BTEType> {
            { typeof(BTEDictionary), BTEType.Dictionary },
            { typeof(BTEList), BTEType.List },
            { typeof(BTEByte), BTEType.Byte },
            { typeof(BTEShort), BTEType.Short },
            { typeof(BTEInt), BTEType.Int },
            { typeof(BTELong), BTEType.Long },
            { typeof(BTEFloat), BTEType.Float },
            { typeof(BTEDouble), BTEType.Double },
            { typeof(BTEString), BTEType.String },
            { typeof(BTEStruct), BTEType.Struct },
            { typeof(BTEByteArray), BTEType.ByteArray },
            { typeof(BTEShortArray), BTEType.ShortArray },
            { typeof(BTEIntArray), BTEType.IntArray },
            { typeof(BTELongArray), BTEType.LongArray },
            { typeof(BTEFloatArray), BTEType.FloatArray },
            { typeof(BTEDoubleArray), BTEType.DoubleArray },
            { typeof(BTEStringArray), BTEType.StringArray },
            { typeof(BTEStructArray), BTEType.StructArray },
            { typeof(BTEGuid), BTEType.Guid }
        };
    }

    protected BinaryTreeElement() {
    }

    /// <summary>
    /// Reads this element's data from the given binary reader. This may be a recursive operation (for lists, dictionaries, etc)
    /// </summary>
    /// <param name="reader">The reader (data source)</param>
    protected abstract void Read(BinaryReader reader);

    /// <summary>
    /// Writes this element's data into the given binary writer. This may be a recursive operation (for lists, dictionaries, etc)
    /// </summary>
    /// <param name="writer">The writer (data target)</param>
    protected abstract void Write(BinaryWriter writer);

    /// <summary>
    /// Reads this element's data in packed form from the given binary reader. By default, this just invokes <see cref="Read"/>.
    /// Collection based elements are the only ones that need to override this, as they form a recursive read operation
    /// </summary>
    /// <param name="reader">The reader (data source)</param>
    /// <param name="packData">
    /// The dictionary which maps a key index to the actual string key (used by dictionary based elements, like <see cref="BTEDictionary"/>)
    /// </param>
    protected virtual void ReadPacked(BinaryReader reader, Dictionary<int, string> packData) {
        this.Read(reader);
    }

    /// <summary>
    /// Writes this element's data, in packed form, into the given binary writer. By default, this just invokes <see cref="Write"/>.
    /// Collection based elements are the only ones that need to override this, as they form a recursive write operation
    /// </summary>
    /// <param name="writer">The writer (data target)</param>
    /// <param name="dictionary">
    /// A pre-computed dictionary which maps all string keys to an index which should
    /// be written instead of the actual key (used by dictionary based elements, like <see cref="BTEDictionary"/>)
    /// </param>
    protected virtual void WritePacked(BinaryWriter writer, Dictionary<string, int> dictionary) {
        this.Write(writer);
    }

    /// <summary>
    /// Accumulates all of the keys that this element uses. This is a recursive operation, and is invoked before any elements are
    /// written via <see cref="WritePacked"/>. Entries added to the dictionary should set the index as the dictionary's current count,
    /// which saves passing an integer reference
    /// </summary>
    /// <param name="dictionary">The dictionary, in which entries should be added to</param>
    protected internal virtual void AccumulatePackedEntries(Dictionary<string, int> dictionary) {
    }

    /// <summary>
    /// Creates a deep clone of this element
    /// </summary>
    /// <returns>A new element which contains no references (at all) to the instance that was originally cloned</returns>
    public abstract BinaryTreeElement Clone();

    public override string ToString() {
        return TryGetIdByType(this.GetType(), out BTEType type) ? type.ToString() : this.GetType().ToString();
    }

    /// <summary>
    /// Reads an BTE object from the given binary reader
    /// </summary>
    /// <param name="reader">Binary data source</param>
    /// <returns></returns>
    public static BinaryTreeElement ReadIdAndElement(BinaryReader reader) {
        byte id = reader.ReadByte();
        BinaryTreeElement element = CreateById((BTEType) id);
        element.Read(reader);
        return element;
    }

    public static void WriteIdAndElement(BinaryWriter writer, BinaryTreeElement element) {
        writer.Write((byte) element.Type);
        element.Write(writer);
    }

    public static BinaryTreeElement ReadIdAndElementPacked(BinaryReader reader, Dictionary<int, string> dictionary) {
        byte id = reader.ReadByte();
        BinaryTreeElement element = CreateById((BTEType) id);
        element.ReadPacked(reader, dictionary);
        return element;
    }

    public static void WriteIdAndElementPacked(BinaryWriter writer, BinaryTreeElement element, Dictionary<string, int> dictionary) {
        writer.Write((byte) element.Type);
        element.WritePacked(writer, dictionary);
    }

    public static BinaryTreeElement CreateById(BTEType type) {
        switch (type) {
            case BTEType.Dictionary:  return new BTEDictionary();
            case BTEType.List:        return new BTEList();
            case BTEType.Byte:        return new BTEByte();
            case BTEType.Short:       return new BTEShort();
            case BTEType.Int:         return new BTEInt();
            case BTEType.Long:        return new BTELong();
            case BTEType.Float:       return new BTEFloat();
            case BTEType.Double:      return new BTEDouble();
            case BTEType.String:      return new BTEString();
            case BTEType.Struct:      return new BTEStruct();
            case BTEType.ByteArray:   return new BTEByteArray();
            case BTEType.ShortArray:  return new BTEShortArray();
            case BTEType.IntArray:    return new BTEIntArray();
            case BTEType.LongArray:   return new BTELongArray();
            case BTEType.FloatArray:  return new BTEFloatArray();
            case BTEType.DoubleArray: return new BTEDoubleArray();
            case BTEType.StringArray: return new BTEStringArray();
            case BTEType.StructArray: return new BTEStructArray();
            case BTEType.Guid:        return new BTEGuid();
            default:                  throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown element type: " + type);
        }
    }

    public static bool TryGetIdByType(Type type, out BTEType bteType) => TypeToIdTable.TryGetValue(type, out bteType);

    public static Type GetTypeById(BTEType type) {
        switch (type) {
            case BTEType.Dictionary:  return typeof(BTEDictionary);
            case BTEType.List:        return typeof(BTEList);
            case BTEType.Byte:        return typeof(BTEByte);
            case BTEType.Short:       return typeof(BTEShort);
            case BTEType.Int:         return typeof(BTEInt);
            case BTEType.Long:        return typeof(BTELong);
            case BTEType.Float:       return typeof(BTEFloat);
            case BTEType.Double:      return typeof(BTEDouble);
            case BTEType.String:      return typeof(BTEString);
            case BTEType.Struct:      return typeof(BTEStruct);
            case BTEType.ByteArray:   return typeof(BTEByteArray);
            case BTEType.ShortArray:  return typeof(BTEShortArray);
            case BTEType.IntArray:    return typeof(BTEIntArray);
            case BTEType.LongArray:   return typeof(BTELongArray);
            case BTEType.FloatArray:  return typeof(BTEFloatArray);
            case BTEType.DoubleArray: return typeof(BTEDoubleArray);
            case BTEType.StringArray: return typeof(BTEStringArray);
            case BTEType.StructArray: return typeof(BTEStructArray);
            case BTEType.Guid:        return typeof(BTEGuid);
            default:                  throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown element type: " + type);
        }
    }

    protected static string GetReadableTypeName(Type type) {
        return TryGetIdByType(type, out BTEType elementType) ? elementType.ToString() : type.Name;
    }
}