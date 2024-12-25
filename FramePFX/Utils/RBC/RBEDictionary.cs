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

namespace FramePFX.Utils.RBC;

/// <summary>
/// Used to store named elements using a dictionary
/// </summary>
public class RBEDictionary : RBEBase {
    public Dictionary<string, RBEBase> Map { get; private set; }

    public override RBEType Type => RBEType.Dictionary;

    public RBEBase this[string key] {
        get => this.Map.TryGetValue(ValidateKey(key), out RBEBase value) ? value : null;
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

    public RBEDictionary() {
        this.Map = new Dictionary<string, RBEBase>();
    }

    public RBEDictionary(Dictionary<string, RBEBase> map) {
        this.Map = map ?? throw new ArgumentNullException(nameof(map), "Map cannot be null");
    }

    #region Getters And Setters (and similar util functions)

    public bool ContainsKey(string key) => this.Map.ContainsKey(ValidateKey(key));

    public T GetElement<T>(string key) where T : RBEBase {
        if (this.TryGetElement(key, out T? value)) {
            return value;
        }

        string readableTypeName = TryGetIdByType(typeof(T), out RBEType type) ? type.ToString() : typeof(T).ToString();
        throw new Exception($"No such entry '{key}' of type {readableTypeName}");
    }

    public bool TryGetElement<T>(string key, [NotNullWhen(true)] out T? element) where T : RBEBase {
        if (this.Map.TryGetValue(ValidateKey(key), out RBEBase? rbeBase) && rbeBase is T rbe) {
            element = rbe;
            return true;
        }

        element = default;
        return false;
    }

    public bool TryGetElementValue<TElement, T>(string key, Func<TElement, T?> elemToVal, out T? value) where TElement : RBEBase {
        if (this.TryGetElement(key, out TElement? element)) {
            value = elemToVal(element);
            return true;
        }

        value = default;
        return false;
    }

    public RBEDictionary GetDictionary(string key) => this.GetElement<RBEDictionary>(key);
    public RBEDictionary GetDictionary(string key, RBEDictionary def) => this.TryGetElement(key, out RBEDictionary? rbe) ? rbe : def;
    public bool TryGetDictionary(string key, out RBEDictionary value) => (value = this.GetDictionary(key, null)) != null;

    public RBEDictionary GetOrCreateDictionary(string key) {
        if (!this.TryGetElement(key, out RBEDictionary? dictionary))
            this[key] = dictionary = new RBEDictionary();
        return dictionary;
    }

    public RBEDictionary CreateDictionary(string key) {
        if (this.ContainsKey(key))
            throw new Exception("Key already in use: " + key);
        RBEDictionary dictionary = new RBEDictionary();
        this[key] = dictionary;
        return dictionary;
    }

    public RBEList GetList(string key) => this.GetElement<RBEList>(key);
    public RBEList GetList(string key, RBEList def) => this.TryGetElement(key, out RBEList? rbe) ? rbe : def;
    public bool TryGetList(string key, out RBEList value) => (value = this.GetList(key, null)) != null;

    public RBEList GetOrCreateList(string key) {
        if (!this.TryGetElement(key, out RBEList? dictionary))
            this[key] = dictionary = new RBEList();
        return dictionary;
    }

    public RBEList CreateList(string key) {
        if (this.ContainsKey(key))
            throw new Exception("Key already in use: " + key);
        RBEList list = new RBEList();
        this[key] = list;
        return list;
    }

    public bool GetBool(string key) => this.GetByte(key) != 0;
    public bool GetBool(string key, bool def) => this.TryGetElement(key, out RBEByte? rbe) ? (rbe.Value != 0) : def;
    public bool TryGetBool(string key, out bool value) => this.TryGetElementValue<RBEByte, bool>(key, e => e.Value != 0, out value);

    // These are kinda pointless since you can just cast to/from the appropriately sized integer, but still

    public T GetEnum8<T>(string key) where T : unmanaged, Enum => BinaryUtils.ToEnum8<T>(this.GetByte(key));
    public T GetEnum8<T>(string key, T def) where T : unmanaged, Enum => this.TryGetElement(key, out RBEByte? rbe) ? BinaryUtils.ToEnum8<T>(rbe.Value) : def;
    public bool TryGetEnum8<T>(string key, out T value) where T : unmanaged, Enum => this.TryGetElementValue<RBEByte, T>(key, e => BinaryUtils.ToEnum8<T>(e.Value), out value);

    public T GetEnum16<T>(string key) where T : unmanaged, Enum => BinaryUtils.ToEnum16<T>(this.GetByte(key));
    public T GetEnum16<T>(string key, T def) where T : unmanaged, Enum => this.TryGetElement(key, out RBEByte? rbe) ? BinaryUtils.ToEnum16<T>(rbe.Value) : def;
    public bool TryGetEnum16<T>(string key, out T value) where T : unmanaged, Enum => this.TryGetElementValue<RBEByte, T>(key, e => BinaryUtils.ToEnum16<T>(e.Value), out value);

    public T GetEnum32<T>(string key) where T : unmanaged, Enum => BinaryUtils.ToEnum32<T>(this.GetByte(key));
    public T GetEnum32<T>(string key, T def) where T : unmanaged, Enum => this.TryGetElement(key, out RBEByte? rbe) ? BinaryUtils.ToEnum32<T>(rbe.Value) : def;
    public bool TryGetEnum32<T>(string key, out T value) where T : unmanaged, Enum => this.TryGetElementValue<RBEByte, T>(key, e => BinaryUtils.ToEnum32<T>(e.Value), out value);

    public T GetEnum64<T>(string key) where T : unmanaged, Enum => BinaryUtils.ToEnum64<T>(this.GetByte(key));
    public T GetEnum64<T>(string key, T def) where T : unmanaged, Enum => this.TryGetElement(key, out RBEByte? rbe) ? BinaryUtils.ToEnum64<T>(rbe.Value) : def;
    public bool TryGetEnum64<T>(string key, out T value) where T : unmanaged, Enum => this.TryGetElementValue<RBEByte, T>(key, e => BinaryUtils.ToEnum64<T>(e.Value), out value);

    public byte GetByte(string key) => this.GetElement<RBEByte>(key).Value;
    public byte GetByte(string key, byte def) => this.TryGetElement(key, out RBEByte? rbe) ? rbe.Value : def;
    public bool TryGetByte(string key, out byte value) => this.TryGetElementValue<RBEByte, byte>(key, e => e.Value, out value);

    public short GetShort(string key) => this.GetElement<RBEShort>(key).Value;
    public short GetShort(string key, short def) => this.TryGetElement(key, out RBEShort? rbe) ? rbe.Value : def;
    public bool TryGetShort(string key, out short value) => this.TryGetElementValue<RBEShort, short>(key, e => e.Value, out value);

    public int GetInt(string key) => this.GetElement<RBEInt>(key).Value;
    public int GetInt(string key, int def) => this.TryGetElement(key, out RBEInt? rbe) ? rbe.Value : def;
    public bool TryGetInt(string key, out int value) => this.TryGetElementValue<RBEInt, int>(key, e => e.Value, out value);

    public uint GetUInt(string key) => (uint) this.GetElement<RBEInt>(key).Value;
    public uint GetUInt(string key, uint def) => this.TryGetElement(key, out RBEInt? rbe) ? (uint) rbe.Value : def;
    public bool TryGetUInt(string key, out uint value) => this.TryGetElementValue<RBEInt, uint>(key, e => (uint) e.Value, out value);

    public long GetLong(string key) => this.GetElement<RBELong>(key).Value;
    public long GetLong(string key, long def) => this.TryGetElement(key, out RBELong? rbe) ? rbe.Value : def;
    public bool TryGetLong(string key, out long value) => this.TryGetElementValue<RBELong, long>(key, e => e.Value, out value);

    public ulong GetULong(string key) => (ulong) this.GetElement<RBELong>(key).Value;
    public ulong GetULong(string key, ulong def) => this.TryGetElement(key, out RBELong? rbe) ? (ulong) rbe.Value : def;
    public bool TryGetULong(string key, out ulong value) => this.TryGetElementValue<RBELong, ulong>(key, e => (ulong) e.Value, out value);

    public float GetFloat(string key) => this.GetElement<RBEFloat>(key).Value;
    public float GetFloat(string key, float def) => this.TryGetElement(key, out RBEFloat? rbe) ? rbe.Value : def;
    public bool TryGetFloat(string key, out float value) => this.TryGetElementValue<RBEFloat, float>(key, e => e.Value, out value);

    public double GetDouble(string key) => this.GetElement<RBEDouble>(key).Value;
    public double GetDouble(string key, double def) => this.TryGetElement(key, out RBEDouble? rbe) ? rbe.Value : def;
    public bool TryGetDouble(string key, out double value) => this.TryGetElementValue<RBEDouble, double>(key, e => e.Value, out value);

    public string? GetString(string key) => this.GetElement<RBEString>(key).Value;

    [return: NotNullIfNotNull(nameof(def))]
    public string? GetString(string key, string? def) => this.TryGetElement(key, out RBEString? rbe) ? rbe.Value : def;

    public bool TryGetString(string key, [NotNullWhen(true)] out string? value) => this.TryGetElementValue<RBEString, string>(key, e => e.Value, out value);

    // public string GetLongString(string key) => GetString(this.GetElement<RBEList>(key).List);
    // public string GetLongString(string key, string def) => this.TryGetElement(key, out RBEList rbe) ? GetString(rbe.List) : def;
    // public bool TryGetLongString(string key, out string value) => this.TryGetElementValue<RBEList, string>(key, e => GetString(e.List), out value);

    // private static string GetString(List<RBEBase> list) {
    //     StringBuilder sb = new StringBuilder(ushort.MaxValue * 2);
    //     foreach (RBEBase rbe in list) {
    //         if (!(rbe is RBEString))
    //             throw new Exception("Expected list to contain only string elements");
    //         sb.Append(((RBEString) rbe).Value);
    //     }
    //     return sb.ToString();
    // }

    // private static void SetString(RBEList list, string value) {
    //     int i = 0, j, c = value.Length;
    //     do {
    //         j = i;
    //         i += ushort.MaxValue;
    //         list.Add(new RBEString(value.JSubstring(j, Math.Min(i, c))));
    //     } while (i < c);
    // }

    public T GetStruct<T>(string key) where T : unmanaged => this.GetElement<RBEStruct>(key).GetValue<T>();
    public T GetStruct<T>(string key, T def) where T : unmanaged => this.TryGetElement(key, out RBEStruct? rbe) ? rbe.GetValue<T>() : def;

    public bool TryGetStruct<T>(string key, out T value) where T : unmanaged {
        if (this.TryGetElement(key, out RBEStruct? rbe) && rbe.TryGetValue(out value))
            return true;
        value = default;
        return false;
    }

    public byte[]? GetByteArray(string key) => this.GetElement<RBEByteArray>(key).Array;

    [return: NotNullIfNotNull(nameof(def))]
    public byte[]? GetByteArray(string key, byte[]? def) => this.TryGetElement(key, out RBEByteArray? rbe) ? rbe.Array : def;

    public bool TryGetByteArray(string key, out byte[]? value) => this.TryGetElementValue<RBEByteArray, byte[]>(key, e => e.Array, out value);

    public short[]? GetShortArray(string key) => this.GetElement<RBEShortArray>(key).Array;

    [return: NotNullIfNotNull(nameof(def))]
    public short[]? GetShortArray(string key, short[]? def) => this.TryGetElement(key, out RBEShortArray? rbe) ? rbe.Array : def;

    public bool TryGetShortArray(string key, out short[]? value) => this.TryGetElementValue<RBEShortArray, short[]>(key, e => e.Array, out value);

    public int[]? GetIntArray(string key) => this.GetElement<RBEIntArray>(key).Array;

    [return: NotNullIfNotNull(nameof(def))]
    public int[]? GetIntArray(string key, int[]? def) => this.TryGetElement(key, out RBEIntArray? rbe) ? rbe.Array : def;

    public bool TryGetIntArray(string key, out int[]? value) => this.TryGetElementValue<RBEIntArray, int[]>(key, e => e.Array, out value);

    public long[]? GetLongArray(string key) => this.GetElement<RBELongArray>(key).Array;

    [return: NotNullIfNotNull(nameof(def))]
    public long[]? GetLongArray(string key, long[]? def) => this.TryGetElement(key, out RBELongArray? rbe) ? rbe.Array : def;

    public bool TryGetLongArray(string key, out long[]? value) => this.TryGetElementValue<RBELongArray, long[]>(key, e => e.Array, out value);

    public float[]? GetFloatArray(string key) => this.GetElement<RBEFloatArray>(key).Array;

    [return: NotNullIfNotNull(nameof(def))]
    public float[]? GetFloatArray(string key, float[]? def) => this.TryGetElement(key, out RBEFloatArray? rbe) ? rbe.Array : def;

    public bool TryGetFloatArray(string key, out float[]? value) => this.TryGetElementValue<RBEFloatArray, float[]>(key, e => e.Array, out value);

    public double[]? GetDoubleArray(string key) => this.GetElement<RBEDoubleArray>(key).Array;

    [return: NotNullIfNotNull(nameof(def))]
    public double[]? GetDoubleArray(string key, double[]? def) => this.TryGetElement(key, out RBEDoubleArray? rbe) ? rbe.Array : def;

    public bool TryGetDoubleArray(string key, out double[]? value) => this.TryGetElementValue<RBEDoubleArray, double[]>(key, e => e.Array, out value);

    public string[]? GetStringArray(string key) => this.GetElement<RBEStringArray>(key).Array;

    [return: NotNullIfNotNull(nameof(def))]
    public string[]? GetStringArray(string key, string[]? def) => this.TryGetElement(key, out RBEStringArray? rbe) ? rbe.Array : def;

    public bool TryGetStringArray(string key, out string[]? value) => this.TryGetElementValue<RBEStringArray, string[]>(key, e => e.Array, out value);

    public T[] GetStructArray<T>(string key) where T : unmanaged => this.GetElement<RBEStructArray>(key).GetValues<T>();
    public T[] GetStructArray<T>(string key, T[] def) where T : unmanaged => this.TryGetElement(key, out RBEStructArray? rbe) ? rbe.GetValues<T>() : def;

    public bool TryGetStructArray<T>(string key, out T[] value) where T : unmanaged {
        if (this.TryGetElement(key, out RBEStructArray? rbe) && rbe.TryGetValues(out value)) {
            return true;
        }

        value = default;
        return false;
    }

    public Guid GetGuid(string key) => this.GetElement<RBEGuid>(key).Value;
    public Guid GetGuid(string key, Guid def) => this.TryGetElement(key, out RBEGuid? rbe) ? rbe.Value : def;
    public bool TryGetGuid(string key, out Guid value) => this.TryGetElementValue<RBEGuid, Guid>(key, e => e.Value, out value);

    public void SetDictionary(string key, Dictionary<string, RBEBase> value) => this[key] = new RBEDictionary(value);
    public void SetList(string key, List<RBEBase> value) => this[key] = new RBEList(value);
    public void SetBool(string key, bool value) => this[key] = new RBEByte((byte) (value ? 1 : 0));
    public void SetEnum8<T>(string key, T value) where T : unmanaged, Enum => this.SetByte(key, BinaryUtils.FromEnum8(value));
    public void SetEnum16<T>(string key, T value) where T : unmanaged, Enum => this.SetShort(key, BinaryUtils.FromEnum16(value));
    public void SetEnum32<T>(string key, T value) where T : unmanaged, Enum => this.SetInt(key, BinaryUtils.FromEnum32(value));
    public void SetEnum64<T>(string key, T value) where T : unmanaged, Enum => this.SetLong(key, BinaryUtils.FromEnum64(value));
    public void SetByte(string key, byte value) => this[key] = new RBEByte(value);
    public void SetShort(string key, short value) => this[key] = new RBEShort(value);
    public void SetInt(string key, int value) => this[key] = new RBEInt(value);
    public void SetUInt(string key, uint value) => this.SetInt(key, (int) value);
    public void SetLong(string key, long value) => this[key] = new RBELong(value);
    public void SetULong(string key, ulong value) => this.SetLong(key, (long) value);
    public void SetFloat(string key, float value) => this[key] = new RBEFloat(value);
    public void SetDouble(string key, double value) => this[key] = new RBEDouble(value);
    public void SetString(string key, string? value) => this[key] = new RBEString(value);
    public void SetStruct<T>(string key, in T value) where T : unmanaged => this[key] = RBEStruct.ForValue(in value);
    public void SetByteArray(string key, byte[] array) => this[key] = new RBEByteArray(array);
    public void SetShortArray(string key, short[] array) => this[key] = new RBEShortArray(array);
    public void SetIntArray(string key, int[] array) => this[key] = new RBEIntArray(array);
    public void SetLongArray(string key, long[] array) => this[key] = new RBELongArray(array);
    public void SetFloatArray(string key, float[] array) => this[key] = new RBEFloatArray(array);
    public void SetDoubleArray(string key, double[] array) => this[key] = new RBEDoubleArray(array);
    public void SetStringArray(string key, string[] array) => this[key] = new RBEStringArray(array);
    public void SetStructArray<T>(string key, T[] array) where T : unmanaged => this[key] = RBEStructArray.ForValues(array);
    public void SetGuid(string key, Guid value) => this[key] = new RBEGuid(value);

    #endregion

    protected override void Read(BinaryReader reader) {
        int length = reader.ReadUInt16();
        this.Map = new Dictionary<string, RBEBase>(length);
        for (int i = 0; i < length; i++) {
            string key = new string(reader.ReadChars(reader.ReadByte()));
            RBEBase element = ReadIdAndElement(reader);
            this.Map[key] = element;
        }
    }

    protected override void ReadPacked(BinaryReader reader, Dictionary<int, string> packData) {
        int length = reader.ReadUInt16();
        this.Map = new Dictionary<string, RBEBase>(length);
        for (int i = 0; i < length; i++) {
            int index = reader.ReadInt32();
            if (!packData.TryGetValue(index, out string? key))
                throw new Exception($"No such key for index: {index}");
            RBEBase element = ReadIdAndElementPacked(reader, packData);
            this.Map[key] = element;
        }
    }

    protected override void Write(BinaryWriter writer) {
        writer.Write((ushort) this.Map.Count);
        foreach (KeyValuePair<string, RBEBase> entry in this.Map) {
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
        foreach (KeyValuePair<string, RBEBase> entry in this.Map) {
            if (!dictionary.TryGetValue(entry.Key, out int index))
                throw new Exception($"No such index for key: {entry.Key}");
            writer.Write(index);
            WriteIdAndElementPacked(writer, entry.Value, dictionary);
        }
    }

    protected internal override void AccumulatePackedEntries(Dictionary<string, int> dictionary) {
        foreach (KeyValuePair<string, RBEBase> entry in this.Map) {
            if (!dictionary.ContainsKey(entry.Key))
                dictionary[entry.Key] = dictionary.Count;
            entry.Value.AccumulatePackedEntries(dictionary);
        }
    }

    public override RBEBase Clone() => this.CloneCore();

    public RBEDictionary CloneCore() {
        Dictionary<string, RBEBase> map = new Dictionary<string, RBEBase>(this.Map.Count);
        foreach (KeyValuePair<string, RBEBase> element in this.Map)
            map[element.Key] = element.Value.Clone();
        return new RBEDictionary(map);
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