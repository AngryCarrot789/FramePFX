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

using System.Diagnostics.CodeAnalysis;

namespace FramePFX.Utils.BTE;

/// <summary>
/// Used to store named elements using a dictionary
/// </summary>
public class BTEDictionary : BinaryTreeElement {
    public Dictionary<string, BinaryTreeElement> Map { get; private set; }

    public override BTEType Type => BTEType.Dictionary;

    public BinaryTreeElement this[string key] {
        get => this.Map.TryGetValue(ValidateKey(key), out BinaryTreeElement value) ? value : null;
        set {
            key = ValidateKey(key);
            if (value != null) {
                this.Map[key] = value;
            }
            else {
                this.Map.Remove(key);
            }
        }
    }

    public BTEDictionary() {
        this.Map = new Dictionary<string, BinaryTreeElement>();
    }

    public BTEDictionary(Dictionary<string, BinaryTreeElement> map) {
        this.Map = map ?? throw new ArgumentNullException(nameof(map), "Map cannot be null");
    }

    #region Getters And Setters (and similar util functions)

    public bool ContainsKey(string key) => this.Map.ContainsKey(ValidateKey(key));

    public T GetElement<T>(string key) where T : BinaryTreeElement {
        if (this.TryGetElement(key, out T? value)) {
            return value;
        }

        string readableTypeName = TryGetIdByType(typeof(T), out BTEType type) ? type.ToString() : typeof(T).ToString();
        throw new Exception($"No such entry '{key}' of type {readableTypeName}");
    }

    public bool TryGetElement<T>(string key, [NotNullWhen(true)] out T? element) where T : BinaryTreeElement {
        if (this.Map.TryGetValue(ValidateKey(key), out BinaryTreeElement? elemBase) && elemBase is T t) {
            element = t;
            return true;
        }

        element = default;
        return false;
    }

    public bool TryGetElementValue<TElement, T>(string key, Func<TElement, T?> elemToVal, out T? value) where TElement : BinaryTreeElement {
        if (this.TryGetElement(key, out TElement? element)) {
            value = elemToVal(element);
            return true;
        }

        value = default;
        return false;
    }

    public BTEDictionary GetDictionary(string key) => this.GetElement<BTEDictionary>(key);
    public BTEDictionary GetDictionary(string key, BTEDictionary def) => this.TryGetElement(key, out BTEDictionary? bte) ? bte : def;
    public bool TryGetDictionary(string key, out BTEDictionary value) => (value = this.GetDictionary(key, null)) != null;

    public BTEDictionary GetOrCreateDictionary(string key) {
        if (!this.TryGetElement(key, out BTEDictionary? dictionary))
            this[key] = dictionary = new BTEDictionary();
        return dictionary;
    }

    public BTEDictionary CreateDictionary(string key) {
        if (this.ContainsKey(key))
            throw new Exception("Key already in use: " + key);
        BTEDictionary dictionary = new BTEDictionary();
        this[key] = dictionary;
        return dictionary;
    }

    public BTEList GetList(string key) => this.GetElement<BTEList>(key);
    public BTEList GetList(string key, BTEList def) => this.TryGetElement(key, out BTEList? bte) ? bte : def;
    public bool TryGetList(string key, out BTEList value) => (value = this.GetList(key, null)) != null;

    public BTEList GetOrCreateList(string key) {
        if (!this.TryGetElement(key, out BTEList? dictionary))
            this[key] = dictionary = new BTEList();
        return dictionary;
    }

    public BTEList CreateList(string key) {
        if (this.ContainsKey(key))
            throw new Exception("Key already in use: " + key);
        BTEList list = new BTEList();
        this[key] = list;
        return list;
    }

    public bool GetBool(string key) => this.GetByte(key) != 0;
    public bool GetBool(string key, bool def) => this.TryGetElement(key, out BTEByte? bte) ? (bte.Value != 0) : def;
    public bool TryGetBool(string key, out bool value) => this.TryGetElementValue<BTEByte, bool>(key, e => e.Value != 0, out value);

    // These are kinda pointless since you can just cast to/from the appropriately sized integer, but still

    public T GetEnum8<T>(string key) where T : unmanaged, Enum => BinaryUtils.ToEnum8<T>(this.GetByte(key));
    public T GetEnum8<T>(string key, T def) where T : unmanaged, Enum => this.TryGetElement(key, out BTEByte? bte) ? BinaryUtils.ToEnum8<T>(bte.Value) : def;
    public bool TryGetEnum8<T>(string key, out T value) where T : unmanaged, Enum => this.TryGetElementValue<BTEByte, T>(key, e => BinaryUtils.ToEnum8<T>(e.Value), out value);

    public T GetEnum16<T>(string key) where T : unmanaged, Enum => BinaryUtils.ToEnum16<T>(this.GetByte(key));
    public T GetEnum16<T>(string key, T def) where T : unmanaged, Enum => this.TryGetElement(key, out BTEByte? bte) ? BinaryUtils.ToEnum16<T>(bte.Value) : def;
    public bool TryGetEnum16<T>(string key, out T value) where T : unmanaged, Enum => this.TryGetElementValue<BTEByte, T>(key, e => BinaryUtils.ToEnum16<T>(e.Value), out value);

    public T GetEnum32<T>(string key) where T : unmanaged, Enum => BinaryUtils.ToEnum32<T>(this.GetByte(key));
    public T GetEnum32<T>(string key, T def) where T : unmanaged, Enum => this.TryGetElement(key, out BTEByte? bte) ? BinaryUtils.ToEnum32<T>(bte.Value) : def;
    public bool TryGetEnum32<T>(string key, out T value) where T : unmanaged, Enum => this.TryGetElementValue<BTEByte, T>(key, e => BinaryUtils.ToEnum32<T>(e.Value), out value);

    public T GetEnum64<T>(string key) where T : unmanaged, Enum => BinaryUtils.ToEnum64<T>(this.GetByte(key));
    public T GetEnum64<T>(string key, T def) where T : unmanaged, Enum => this.TryGetElement(key, out BTEByte? bte) ? BinaryUtils.ToEnum64<T>(bte.Value) : def;
    public bool TryGetEnum64<T>(string key, out T value) where T : unmanaged, Enum => this.TryGetElementValue<BTEByte, T>(key, e => BinaryUtils.ToEnum64<T>(e.Value), out value);

    public byte GetByte(string key) => this.GetElement<BTEByte>(key).Value;
    public byte GetByte(string key, byte def) => this.TryGetElement(key, out BTEByte? bte) ? bte.Value : def;
    public bool TryGetByte(string key, out byte value) => this.TryGetElementValue<BTEByte, byte>(key, e => e.Value, out value);

    public short GetShort(string key) => this.GetElement<BTEShort>(key).Value;
    public short GetShort(string key, short def) => this.TryGetElement(key, out BTEShort? bte) ? bte.Value : def;
    public bool TryGetShort(string key, out short value) => this.TryGetElementValue<BTEShort, short>(key, e => e.Value, out value);

    public int GetInt(string key) => this.GetElement<BTEInt>(key).Value;
    public int GetInt(string key, int def) => this.TryGetElement(key, out BTEInt? bte) ? bte.Value : def;
    public bool TryGetInt(string key, out int value) => this.TryGetElementValue<BTEInt, int>(key, e => e.Value, out value);

    public uint GetUInt(string key) => (uint) this.GetElement<BTEInt>(key).Value;
    public uint GetUInt(string key, uint def) => this.TryGetElement(key, out BTEInt? bte) ? (uint) bte.Value : def;
    public bool TryGetUInt(string key, out uint value) => this.TryGetElementValue<BTEInt, uint>(key, e => (uint) e.Value, out value);

    public long GetLong(string key) => this.GetElement<BTELong>(key).Value;
    public long GetLong(string key, long def) => this.TryGetElement(key, out BTELong? bte) ? bte.Value : def;
    public bool TryGetLong(string key, out long value) => this.TryGetElementValue<BTELong, long>(key, e => e.Value, out value);

    public ulong GetULong(string key) => (ulong) this.GetElement<BTELong>(key).Value;
    public ulong GetULong(string key, ulong def) => this.TryGetElement(key, out BTELong? bte) ? (ulong) bte.Value : def;
    public bool TryGetULong(string key, out ulong value) => this.TryGetElementValue<BTELong, ulong>(key, e => (ulong) e.Value, out value);

    public float GetFloat(string key) => this.GetElement<BTEFloat>(key).Value;
    public float GetFloat(string key, float def) => this.TryGetElement(key, out BTEFloat? bte) ? bte.Value : def;
    public bool TryGetFloat(string key, out float value) => this.TryGetElementValue<BTEFloat, float>(key, e => e.Value, out value);

    public double GetDouble(string key) => this.GetElement<BTEDouble>(key).Value;
    public double GetDouble(string key, double def) => this.TryGetElement(key, out BTEDouble? bte) ? bte.Value : def;
    public bool TryGetDouble(string key, out double value) => this.TryGetElementValue<BTEDouble, double>(key, e => e.Value, out value);

    public string? GetString(string key) => this.GetElement<BTEString>(key).Value;

    [return: NotNullIfNotNull(nameof(def))]
    public string? GetString(string key, string? def) => this.TryGetElement(key, out BTEString? bte) ? bte.Value : def;

    public bool TryGetString(string key, [NotNullWhen(true)] out string? value) => this.TryGetElementValue<BTEString, string>(key, e => e.Value, out value);

    public T GetStruct<T>(string key) where T : unmanaged => this.GetElement<BTEStruct>(key).GetValue<T>();
    public T GetStruct<T>(string key, T def) where T : unmanaged => this.TryGetElement(key, out BTEStruct? bte) ? bte.GetValue<T>() : def;

    public bool TryGetStruct<T>(string key, out T value) where T : unmanaged {
        if (this.TryGetElement(key, out BTEStruct? bte) && bte.TryGetValue(out value))
            return true;
        value = default;
        return false;
    }

    public byte[]? GetByteArray(string key) => this.GetElement<BTEByteArray>(key).Array;

    [return: NotNullIfNotNull(nameof(def))]
    public byte[]? GetByteArray(string key, byte[]? def) => this.TryGetElement(key, out BTEByteArray? bte) ? bte.Array : def;

    public bool TryGetByteArray(string key, out byte[]? value) => this.TryGetElementValue<BTEByteArray, byte[]>(key, e => e.Array, out value);

    public short[]? GetShortArray(string key) => this.GetElement<BTEShortArray>(key).Array;

    [return: NotNullIfNotNull(nameof(def))]
    public short[]? GetShortArray(string key, short[]? def) => this.TryGetElement(key, out BTEShortArray? bte) ? bte.Array : def;

    public bool TryGetShortArray(string key, out short[]? value) => this.TryGetElementValue<BTEShortArray, short[]>(key, e => e.Array, out value);

    public int[]? GetIntArray(string key) => this.GetElement<BTEIntArray>(key).Array;

    [return: NotNullIfNotNull(nameof(def))]
    public int[]? GetIntArray(string key, int[]? def) => this.TryGetElement(key, out BTEIntArray? bte) ? bte.Array : def;

    public bool TryGetIntArray(string key, out int[]? value) => this.TryGetElementValue<BTEIntArray, int[]>(key, e => e.Array, out value);

    public long[]? GetLongArray(string key) => this.GetElement<BTELongArray>(key).Array;

    [return: NotNullIfNotNull(nameof(def))]
    public long[]? GetLongArray(string key, long[]? def) => this.TryGetElement(key, out BTELongArray? bte) ? bte.Array : def;

    public bool TryGetLongArray(string key, out long[]? value) => this.TryGetElementValue<BTELongArray, long[]>(key, e => e.Array, out value);

    public float[]? GetFloatArray(string key) => this.GetElement<BTEFloatArray>(key).Array;

    [return: NotNullIfNotNull(nameof(def))]
    public float[]? GetFloatArray(string key, float[]? def) => this.TryGetElement(key, out BTEFloatArray? bte) ? bte.Array : def;

    public bool TryGetFloatArray(string key, out float[]? value) => this.TryGetElementValue<BTEFloatArray, float[]>(key, e => e.Array, out value);

    public double[]? GetDoubleArray(string key) => this.GetElement<BTEDoubleArray>(key).Array;

    [return: NotNullIfNotNull(nameof(def))]
    public double[]? GetDoubleArray(string key, double[]? def) => this.TryGetElement(key, out BTEDoubleArray? bte) ? bte.Array : def;

    public bool TryGetDoubleArray(string key, out double[]? value) => this.TryGetElementValue<BTEDoubleArray, double[]>(key, e => e.Array, out value);

    public string[]? GetStringArray(string key) => this.GetElement<BTEStringArray>(key).Array;

    [return: NotNullIfNotNull(nameof(def))]
    public string[]? GetStringArray(string key, string[]? def) => this.TryGetElement(key, out BTEStringArray? bte) ? bte.Array : def;

    public bool TryGetStringArray(string key, out string[]? value) => this.TryGetElementValue<BTEStringArray, string[]>(key, e => e.Array, out value);

    public T[] GetStructArray<T>(string key) where T : unmanaged => this.GetElement<BTEStructArray>(key).GetValues<T>();
    public T[] GetStructArray<T>(string key, T[] def) where T : unmanaged => this.TryGetElement(key, out BTEStructArray? bte) ? bte.GetValues<T>() : def;

    public bool TryGetStructArray<T>(string key, out T[] value) where T : unmanaged {
        if (this.TryGetElement(key, out BTEStructArray? bte) && bte.TryGetValues(out value)) {
            return true;
        }

        value = default;
        return false;
    }

    public Guid GetGuid(string key) => this.GetElement<BTEGuid>(key).Value;
    public Guid GetGuid(string key, Guid def) => this.TryGetElement(key, out BTEGuid? bte) ? bte.Value : def;
    public bool TryGetGuid(string key, out Guid value) => this.TryGetElementValue<BTEGuid, Guid>(key, e => e.Value, out value);

    public void SetDictionary(string key, Dictionary<string, BinaryTreeElement> value) => this[key] = new BTEDictionary(value);
    public void SetList(string key, List<BinaryTreeElement> value) => this[key] = new BTEList(value);
    public void SetBool(string key, bool value) => this[key] = new BTEByte((byte) (value ? 1 : 0));
    public void SetEnum8<T>(string key, T value) where T : unmanaged, Enum => this.SetByte(key, BinaryUtils.FromEnum8(value));
    public void SetEnum16<T>(string key, T value) where T : unmanaged, Enum => this.SetShort(key, BinaryUtils.FromEnum16(value));
    public void SetEnum32<T>(string key, T value) where T : unmanaged, Enum => this.SetInt(key, BinaryUtils.FromEnum32(value));
    public void SetEnum64<T>(string key, T value) where T : unmanaged, Enum => this.SetLong(key, BinaryUtils.FromEnum64(value));
    public void SetByte(string key, byte value) => this[key] = new BTEByte(value);
    public void SetShort(string key, short value) => this[key] = new BTEShort(value);
    public void SetInt(string key, int value) => this[key] = new BTEInt(value);
    public void SetUInt(string key, uint value) => this.SetInt(key, (int) value);
    public void SetLong(string key, long value) => this[key] = new BTELong(value);
    public void SetULong(string key, ulong value) => this.SetLong(key, (long) value);
    public void SetFloat(string key, float value) => this[key] = new BTEFloat(value);
    public void SetDouble(string key, double value) => this[key] = new BTEDouble(value);
    public void SetString(string key, string? value) => this[key] = new BTEString(value);
    public void SetStruct<T>(string key, in T value) where T : unmanaged => this[key] = BTEStruct.ForValue(in value);
    public void SetByteArray(string key, byte[] array) => this[key] = new BTEByteArray(array);
    public void SetShortArray(string key, short[] array) => this[key] = new BTEShortArray(array);
    public void SetIntArray(string key, int[] array) => this[key] = new BTEIntArray(array);
    public void SetLongArray(string key, long[] array) => this[key] = new BTELongArray(array);
    public void SetFloatArray(string key, float[] array) => this[key] = new BTEFloatArray(array);
    public void SetDoubleArray(string key, double[] array) => this[key] = new BTEDoubleArray(array);
    public void SetStringArray(string key, string[] array) => this[key] = new BTEStringArray(array);
    public void SetStructArray<T>(string key, T[] array) where T : unmanaged => this[key] = BTEStructArray.ForValues(array);
    public void SetGuid(string key, Guid value) => this[key] = new BTEGuid(value);

    #endregion

    protected override void Read(BinaryReader reader) {
        int length = reader.ReadUInt16();
        this.Map = new Dictionary<string, BinaryTreeElement>(length);
        for (int i = 0; i < length; i++) {
            string key = new string(reader.ReadChars(reader.ReadByte()));
            BinaryTreeElement element = ReadIdAndElement(reader);
            this.Map[key] = element;
        }
    }

    protected override void ReadPacked(BinaryReader reader, Dictionary<int, string> packData) {
        int length = reader.ReadUInt16();
        this.Map = new Dictionary<string, BinaryTreeElement>(length);
        for (int i = 0; i < length; i++) {
            int index = reader.ReadInt32();
            if (!packData.TryGetValue(index, out string? key))
                throw new Exception($"No such key for index: {index}");
            BinaryTreeElement element = ReadIdAndElementPacked(reader, packData);
            this.Map[key] = element;
        }
    }

    protected override void Write(BinaryWriter writer) {
        writer.Write((ushort) this.Map.Count);
        foreach (KeyValuePair<string, BinaryTreeElement> entry in this.Map) {
            int length = entry.Key.Length;
            if (length > 255)
                throw new Exception($"Map contained a key longer than 255 characters: {length}");
            writer.Write((byte) length);
            writer.Write(entry.Key.ToCharArray());
            WriteIdAndElement(writer, entry.Value);
        }
    }

    protected override void WritePacked(BinaryWriter writer, Dictionary<string, int> dictionary) {
        writer.Write((ushort) this.Map.Count);
        foreach (KeyValuePair<string, BinaryTreeElement> entry in this.Map) {
            if (!dictionary.TryGetValue(entry.Key, out int index))
                throw new Exception($"No such index for key: {entry.Key}");
            writer.Write(index);
            WriteIdAndElementPacked(writer, entry.Value, dictionary);
        }
    }

    protected internal override void AccumulatePackedEntries(Dictionary<string, int> dictionary) {
        foreach (KeyValuePair<string, BinaryTreeElement> entry in this.Map) {
            if (!dictionary.ContainsKey(entry.Key))
                dictionary[entry.Key] = dictionary.Count;
            entry.Value.AccumulatePackedEntries(dictionary);
        }
    }

    public override BinaryTreeElement Clone() => this.CloneCore();

    public BTEDictionary CloneCore() {
        Dictionary<string, BinaryTreeElement> map = new Dictionary<string, BinaryTreeElement>(this.Map.Count);
        foreach (KeyValuePair<string, BinaryTreeElement> element in this.Map)
            map[element.Key] = element.Value.Clone();
        return new BTEDictionary(map);
    }

    private static string ValidateKey(string key) {
        // CanonicalizeKey
        if (key == null) {
            return "";
        }
        else if (key.Length > 255) {
            throw new ArgumentNullException(nameof(key), $"Key length must be less than 256 characters: {key.Length}");
        }
        else {
            return key;
        }
    }
}