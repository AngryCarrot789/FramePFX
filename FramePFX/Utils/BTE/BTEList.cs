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
/// Used to store an ordered list of elements
/// </summary>
public class BTEList : BinaryTreeElement {
    public List<BinaryTreeElement> List { get; private set; }

    public override BTEType Type => BTEType.List;

    public BTEList() : this(new List<BinaryTreeElement>()) {
    }

    public BTEList(List<BinaryTreeElement> children) {
        this.List = children ?? throw new ArgumentNullException(nameof(children), "List cannot be null");
    }

    /// <summary>
    /// Returns an enumerable of <see cref="List"/>, and pre-checks the list to ensure all values are the correc type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public IEnumerable<T> Cast<T>() where T : BinaryTreeElement {
        if (this.List.Any(x => !(x is T))) {
            throw new Exception($"Expected list to contain only {GetReadableTypeName(typeof(T))} instances");
        }

        return this.List.Cast<T>();
    }

    private bool GetElementByType<T>(int index, out T value) where T : BinaryTreeElement {
        if (index >= 0 && index < this.List.Count) {
            if (this.List[index] is T val) {
                value = val;
                return true;
            }
            else {
                throw new Exception($"Incompatible types: Attempted to get element named {index} of type {typeof(T)}, but type {this.List[index]?.GetType()} was found");
            }
        }
        else {
            value = default;
            return false;
        }
    }

    public BTEDictionary GetDictionaryElement(int index) {
        return this.GetElementByType(index, out BTEDictionary bte) ? bte : null;
    }

    public Dictionary<string, BinaryTreeElement> GetDictionary(int index, Dictionary<string, BinaryTreeElement> def = null) {
        return this.GetElementByType(index, out BTEDictionary bte) ? bte.Map : def;
    }

    public bool TryGetDictionary(int index, out Dictionary<string, BinaryTreeElement> value) {
        BTEDictionary element = this.GetDictionaryElement(index);
        value = element?.Map;
        return element != null;
    }

    public BTEList GetListElement(int index) {
        return this.GetElementByType(index, out BTEList bte) ? bte : null;
    }

    public List<BinaryTreeElement> GetList(int index, List<BinaryTreeElement> def = null) {
        return this.GetElementByType(index, out BTEList bte) ? bte.List : def;
    }

    public bool TryGetList(int index, out List<BinaryTreeElement> value) {
        BTEList element = this.GetListElement(index);
        value = element?.List;
        return element != null;
    }

    public BTEByte GetInt8Element(int index) {
        return this.GetElementByType(index, out BTEByte bte) ? bte : null;
    }

    public byte GetInt8(int index, byte def = default) {
        return this.GetElementByType(index, out BTEByte bte) ? bte.Value : def;
    }

    public bool TryGetInt8(int index, out byte value) {
        BTEByte element = this.GetInt8Element(index);
        value = element?.Value ?? default;
        return element != null;
    }

    public BTEShort GetInt16Element(int index) {
        return this.GetElementByType(index, out BTEShort bte) ? bte : null;
    }

    public short GetInt16(int index, short def = default) {
        return this.GetElementByType(index, out BTEShort bte) ? bte.Value : def;
    }

    public bool TryGetInt16(int index, out short value) {
        BTEShort element = this.GetInt16Element(index);
        value = element?.Value ?? default;
        return element != null;
    }

    public BTEInt GetInt32Element(int index) {
        return this.GetElementByType(index, out BTEInt bte) ? bte : null;
    }

    public int GetInt32(int index, int def = default) {
        return this.GetElementByType(index, out BTEInt bte) ? bte.Value : def;
    }

    public bool TryGetInt32(int index, out int value) {
        BTEInt element = this.GetInt32Element(index);
        value = element?.Value ?? default;
        return element != null;
    }

    public BTELong GetInt64Element(int index) {
        return this.GetElementByType(index, out BTELong bte) ? bte : null;
    }

    public long GetInt64(int index, long def = default) {
        return this.GetElementByType(index, out BTELong bte) ? bte.Value : def;
    }

    public bool TryGetInt64(int index, out long value) {
        BTELong element = this.GetInt64Element(index);
        value = element?.Value ?? default;
        return element != null;
    }

    public BTEFloat GetFloatElement(int index) {
        return this.GetElementByType(index, out BTEFloat bte) ? bte : null;
    }

    public float GetFloat(int index, float def = default) {
        return this.GetElementByType(index, out BTEFloat bte) ? bte.Value : def;
    }

    public bool TryGetFloat(int index, out float value) {
        BTEFloat element = this.GetFloatElement(index);
        value = element?.Value ?? default;
        return element != null;
    }

    public BTEDouble GetDoubleElement(int index) {
        return this.GetElementByType(index, out BTEDouble bte) ? bte : null;
    }

    public double GetDouble(int index, double def = default) {
        return this.GetElementByType(index, out BTEDouble bte) ? bte.Value : def;
    }

    public bool TryGetDouble(int index, out double value) {
        BTEDouble element = this.GetDoubleElement(index);
        value = element?.Value ?? default;
        return element != null;
    }

    public BTEStruct GetStructElement(int index) {
        return this.GetElementByType(index, out BTEStruct bte) ? bte : null;
    }

    public BTEStructArray GetStructArrayElement(int index) {
        return this.GetElementByType(index, out BTEStructArray bte) ? bte : null;
    }

    public T GetStruct<T>(int index) where T : unmanaged {
        return this.GetElementByType(index, out BTEStruct value) ? value.GetValue<T>() : default;
    }

    public T GetStruct<T>(int index, T def) where T : unmanaged {
        return this.GetElementByType(index, out BTEStruct value) ? value.GetValue<T>() : def;
    }

    public bool TryGetStruct<T>(int index, out T value) where T : unmanaged {
        BTEStruct element = this.GetStructElement(index);
        value = element?.GetValue<T>() ?? default;
        return element != null;
    }

    public T[] GetStructArray<T>(int index, T[] def = default) where T : unmanaged {
        return this.GetElementByType(index, out BTEStructArray value) ? value.GetValues<T>() : def;
    }

    public bool TryGetStructArray<T>(int index, out T[] value) where T : unmanaged {
        BTEStructArray element = this.GetStructArrayElement(index);
        value = element?.GetValues<T>();
        return value != null;
    }

    public void Add(BinaryTreeElement element) {
        this.List.Add(element);
    }

    public BTEDictionary AddDictionary(Dictionary<string, BinaryTreeElement> value = null) {
        BTEDictionary dictionary = new BTEDictionary(value ?? new Dictionary<string, BinaryTreeElement>());
        this.List.Add(dictionary);
        return dictionary;
    }

    public BTEList AddList(List<BinaryTreeElement> value = null) {
        BTEList list = new BTEList(value ?? new List<BinaryTreeElement>());
        this.List.Add(list);
        return list;
    }

    public void AddInt8(byte value) {
        this.List.Add(new BTEByte(value));
    }

    public void AddInt16(short value) {
        this.List.Add(new BTEShort(value));
    }

    public void AddInt32(int value) {
        this.List.Add(new BTEInt(value));
    }

    public void AddInt64(long value) {
        this.List.Add(new BTELong(value));
    }

    public void AddFloat(float value) {
        this.List.Add(new BTEFloat(value));
    }

    public void AddDouble(double value) {
        this.List.Add(new BTEDouble(value));
    }

    public void AddStruct<T>(in T value) where T : unmanaged {
        BTEStruct obj = new BTEStruct();
        obj.SetValue(value);
        this.List.Add(obj);
    }

    public void AddStructArray<T>(in T[] value) where T : unmanaged {
        BTEStructArray obj = new BTEStructArray();
        obj.SetValues(value);
        this.List.Add(obj);
    }

    protected override void Read(BinaryReader reader) {
        int length = reader.ReadUInt16();
        this.List = new List<BinaryTreeElement>(length);
        for (int i = 0; i < length; i++) {
            this.List.Add(ReadIdAndElement(reader));
        }
    }

    protected override void ReadPacked(BinaryReader reader, Dictionary<int, string> packData) {
        int length = reader.ReadUInt16();
        this.List = new List<BinaryTreeElement>(length);
        for (int i = 0; i < length; i++) {
            this.List.Add(ReadIdAndElementPacked(reader, packData));
        }
    }

    protected override void Write(BinaryWriter writer) {
        writer.Write((ushort) this.List.Count);
        foreach (BinaryTreeElement child in this.List) {
            WriteIdAndElement(writer, child);
        }
    }

    protected override void WritePacked(BinaryWriter writer, Dictionary<string, int> dictionary) {
        writer.Write((ushort) this.List.Count);
        foreach (BinaryTreeElement child in this.List) {
            WriteIdAndElementPacked(writer, child, dictionary);
        }
    }

    protected internal override void AccumulatePackedEntries(Dictionary<string, int> dictionary) {
        foreach (BinaryTreeElement bte in this.List) {
            bte.AccumulatePackedEntries(dictionary);
        }
    }

    public override BinaryTreeElement Clone() => this.CloneCore();

    public BTEList CloneCore() {
        List<BinaryTreeElement> list = new List<BinaryTreeElement>(this.List);
        // not using Select because there's a possibility it causes a stack overflow exception,
        // because there could be a huge chain of elements (lists in lists in lists etc...)
        foreach (BinaryTreeElement element in this.List)
            list.Add(element.Clone());
        return new BTEList(list);
    }
}