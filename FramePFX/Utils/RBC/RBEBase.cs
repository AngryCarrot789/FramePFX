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

namespace FramePFX.Utils.RBC;

/// <summary>
/// The base class for the RBE (REghZy Binary Element... i know right what a sexy acronym)
/// <para>
/// This is used for list/dictionary based binary structures
/// </para>
/// <para>
/// This is based on minecraft's NBT structure, because it's pretty good... for the most part. The
/// primary binary difference is that <see cref="RBEList"/> does not store its child type, whereas
/// minecraft does (for some reason...? lists maintain their order so the dev should know the right types)
/// </para>
/// </summary>
public abstract class RBEBase
{
    private static readonly Dictionary<Type, RBEType> TypeToIdTable;

    /// <summary>
    /// This element's type
    /// </summary>
    public abstract RBEType Type { get; }

    static RBEBase()
    {
        TypeToIdTable = new Dictionary<Type, RBEType>
        {
            { typeof(RBEDictionary), RBEType.Dictionary },
            { typeof(RBEList), RBEType.List },
            { typeof(RBEByte), RBEType.Byte },
            { typeof(RBEShort), RBEType.Short },
            { typeof(RBEInt), RBEType.Int },
            { typeof(RBELong), RBEType.Long },
            { typeof(RBEFloat), RBEType.Float },
            { typeof(RBEDouble), RBEType.Double },
            { typeof(RBEString), RBEType.String },
            { typeof(RBEStruct), RBEType.Struct },
            { typeof(RBEByteArray), RBEType.ByteArray },
            { typeof(RBEShortArray), RBEType.ShortArray },
            { typeof(RBEIntArray), RBEType.IntArray },
            { typeof(RBELongArray), RBEType.LongArray },
            { typeof(RBEFloatArray), RBEType.FloatArray },
            { typeof(RBEDoubleArray), RBEType.DoubleArray },
            { typeof(RBEStringArray), RBEType.StringArray },
            { typeof(RBEStructArray), RBEType.StructArray },
            { typeof(RBEGuid), RBEType.Guid }
        };
    }

    protected RBEBase() {
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
    /// The dictionary which maps a key index to the actual string key (used by dictionary based elements, like <see cref="RBEDictionary"/>)
    /// </param>
    protected virtual void ReadPacked(BinaryReader reader, Dictionary<int, string> packData)
    {
        this.Read(reader);
    }

    /// <summary>
    /// Writes this element's data, in packed form, into the given binary writer. By default, this just invokes <see cref="Write"/>.
    /// Collection based elements are the only ones that need to override this, as they form a recursive write operation
    /// </summary>
    /// <param name="writer">The writer (data target)</param>
    /// <param name="dictionary">
    /// A pre-computed dictionary which maps all string keys to an index which should
    /// be written instead of the actual key (used by dictionary based elements, like <see cref="RBEDictionary"/>)
    /// </param>
    protected virtual void WritePacked(BinaryWriter writer, Dictionary<string, int> dictionary)
    {
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
    public abstract RBEBase Clone();

    public override string ToString()
    {
        return TryGetIdByType(this.GetType(), out RBEType type) ? type.ToString() : this.GetType().ToString();
    }

    /// <summary>
    /// Reads an RBE object from the given binary reader
    /// </summary>
    /// <param name="reader">Binary data source</param>
    /// <returns></returns>
    public static RBEBase ReadIdAndElement(BinaryReader reader)
    {
        byte id = reader.ReadByte();
        RBEBase element = CreateById((RBEType) id);
        element.Read(reader);
        return element;
    }

    public static void WriteIdAndElement(BinaryWriter writer, RBEBase rbe)
    {
        writer.Write((byte) rbe.Type);
        rbe.Write(writer);
    }

    public static RBEBase ReadIdAndElementPacked(BinaryReader reader, Dictionary<int, string> dictionary)
    {
        byte id = reader.ReadByte();
        RBEBase element = CreateById((RBEType) id);
        element.ReadPacked(reader, dictionary);
        return element;
    }

    public static void WriteIdAndElementPacked(BinaryWriter writer, RBEBase rbe, Dictionary<string, int> dictionary)
    {
        writer.Write((byte) rbe.Type);
        rbe.WritePacked(writer, dictionary);
    }

    public static RBEBase CreateById(RBEType id)
    {
        switch (id)
        {
            case RBEType.Dictionary: return new RBEDictionary();
            case RBEType.List: return new RBEList();
            case RBEType.Byte: return new RBEByte();
            case RBEType.Short: return new RBEShort();
            case RBEType.Int: return new RBEInt();
            case RBEType.Long: return new RBELong();
            case RBEType.Float: return new RBEFloat();
            case RBEType.Double: return new RBEDouble();
            case RBEType.String: return new RBEString();
            case RBEType.Struct: return new RBEStruct();
            case RBEType.ByteArray: return new RBEByteArray();
            case RBEType.ShortArray: return new RBEShortArray();
            case RBEType.IntArray: return new RBEIntArray();
            case RBEType.LongArray: return new RBELongArray();
            case RBEType.FloatArray: return new RBEFloatArray();
            case RBEType.DoubleArray: return new RBEDoubleArray();
            case RBEType.StringArray: return new RBEStringArray();
            case RBEType.StructArray: return new RBEStructArray();
            case RBEType.Guid: return new RBEGuid();
            default: throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown RBEType: " + id);
        }
    }

    public static bool TryGetIdByType(Type type, out RBEType rbeType) => TypeToIdTable.TryGetValue(type, out rbeType);

    public static Type GetTypeById(RBEType rbeType)
    {
        switch (rbeType)
        {
            case RBEType.Dictionary: return typeof(RBEDictionary);
            case RBEType.List: return typeof(RBEList);
            case RBEType.Byte: return typeof(RBEByte);
            case RBEType.Short: return typeof(RBEShort);
            case RBEType.Int: return typeof(RBEInt);
            case RBEType.Long: return typeof(RBELong);
            case RBEType.Float: return typeof(RBEFloat);
            case RBEType.Double: return typeof(RBEDouble);
            case RBEType.String: return typeof(RBEString);
            case RBEType.Struct: return typeof(RBEStruct);
            case RBEType.ByteArray: return typeof(RBEByteArray);
            case RBEType.ShortArray: return typeof(RBEShortArray);
            case RBEType.IntArray: return typeof(RBEIntArray);
            case RBEType.LongArray: return typeof(RBELongArray);
            case RBEType.FloatArray: return typeof(RBEFloatArray);
            case RBEType.DoubleArray: return typeof(RBEDoubleArray);
            case RBEType.StringArray: return typeof(RBEStringArray);
            case RBEType.StructArray: return typeof(RBEStructArray);
            case RBEType.Guid: return typeof(RBEGuid);
            default: throw new ArgumentOutOfRangeException(nameof(rbeType), rbeType, "Unknown RBEType: " + rbeType);
        }
    }

    protected static string GetReadableTypeName(Type type)
    {
        return TryGetIdByType(type, out RBEType rbeType) ? rbeType.ToString() : type.Name;
    }
}