using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Core.RBC {
    /// <summary>
    /// Used to store an ordered list of elements
    /// </summary>
    public class RBEList : RBEBase {
        public List<RBEBase> List { get; private set; }

        public override RBEType Type => RBEType.List;

        public RBEList() : this(new List<RBEBase>()) {

        }

        public RBEList(List<RBEBase> children) {
            this.List = children ?? throw new ArgumentNullException(nameof(children), "List cannot be null");
        }

        private bool GetElementByType<T>(int index, out T value) where T : RBEBase {
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

        public RBEDictionary GetDictionaryElement(int index) {
            return this.GetElementByType(index, out RBEDictionary rbe) ? rbe : null;
        }

        public Dictionary<string, RBEBase> GetDictionary(int index, Dictionary<string, RBEBase> def = null) {
            return this.GetElementByType(index, out RBEDictionary rbe) ? rbe.Map : def;
        }

        public bool TryGetDictionary(int index, out Dictionary<string, RBEBase> value) {
            RBEDictionary element = this.GetDictionaryElement(index);
            value = element?.Map;
            return element != null;
        }

        public RBEList GetListElement(int index) {
            return this.GetElementByType(index, out RBEList rbe) ? rbe : null;
        }

        public List<RBEBase> GetList(int index, List<RBEBase> def = null) {
            return this.GetElementByType(index, out RBEList rbe) ? rbe.List : def;
        }

        public bool TryGetList(int index, out List<RBEBase> value) {
            RBEList element = this.GetListElement(index);
            value = element?.List;
            return element != null;
        }

        public RBEByte GetInt8Element(int index) {
            return this.GetElementByType(index, out RBEByte rbe) ? rbe : null;
        }

        public byte GetInt8(int index, byte def = default) {
            return this.GetElementByType(index, out RBEByte rbe) ? rbe.Value : def;
        }

        public bool TryGetInt8(int index, out byte value) {
            RBEByte element = this.GetInt8Element(index);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBEShort GetInt16Element(int index) {
            return this.GetElementByType(index, out RBEShort rbe) ? rbe : null;
        }

        public short GetInt16(int index, short def = default) {
            return this.GetElementByType(index, out RBEShort rbe) ? rbe.Value : def;
        }

        public bool TryGetInt16(int index, out short value) {
            RBEShort element = this.GetInt16Element(index);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBEInt GetInt32Element(int index) {
            return this.GetElementByType(index, out RBEInt rbe) ? rbe : null;
        }

        public int GetInt32(int index, int def = default) {
            return this.GetElementByType(index, out RBEInt rbe) ? rbe.Value : def;
        }

        public bool TryGetInt32(int index, out int value) {
            RBEInt element = this.GetInt32Element(index);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBELong GetInt64Element(int index) {
            return this.GetElementByType(index, out RBELong rbe) ? rbe : null;
        }

        public long GetInt64(int index, long def = default) {
            return this.GetElementByType(index, out RBELong rbe) ? rbe.Value : def;
        }

        public bool TryGetInt64(int index, out long value) {
            RBELong element = this.GetInt64Element(index);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBEFloat GetFloatElement(int index) {
            return this.GetElementByType(index, out RBEFloat rbe) ? rbe : null;
        }

        public float GetFloat(int index, float def = default) {
            return this.GetElementByType(index, out RBEFloat rbe) ? rbe.Value : def;
        }

        public bool TryGetFloat(int index, out float value) {
            RBEFloat element = this.GetFloatElement(index);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBEDouble GetDoubleElement(int index) {
            return this.GetElementByType(index, out RBEDouble rbe) ? rbe : null;
        }

        public double GetDouble(int index, double def = default) {
            return this.GetElementByType(index, out RBEDouble rbe) ? rbe.Value : def;
        }

        public bool TryGetDouble(int index, out double value) {
            RBEDouble element = this.GetDoubleElement(index);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBEStruct GetStructElement(int index) {
            return this.GetElementByType(index, out RBEStruct rbe) ? rbe : null;
        }

        public RBEStructArray GetStructArrayElement(int index) {
            return this.GetElementByType(index, out RBEStructArray rbe) ? rbe : null;
        }

        public T GetStruct<T>(int index) where T : unmanaged {
            return this.GetElementByType(index, out RBEStruct value) ? value.GetValue<T>() : default;
        }

        public T GetStruct<T>(int index, T def) where T : unmanaged {
            return this.GetElementByType(index, out RBEStruct value) ? value.GetValue<T>() : def;
        }

        public bool TryGetStruct<T>(int index, out T value) where T : unmanaged {
            RBEStruct element = this.GetStructElement(index);
            value = element?.GetValue<T>() ?? default;
            return element != null;
        }

        public T[] GetStructArray<T>(int index, T[] def = default) where T : unmanaged {
            return this.GetElementByType(index, out RBEStructArray value) ? value.GetValues<T>() : def;
        }

        public bool TryGetStructArray<T>(int index, out T[] value) where T : unmanaged {
            RBEStructArray element = this.GetStructArrayElement(index);
            value = element?.GetValues<T>();
            return value != null;
        }
        
        public void AddDictionary(Dictionary<string, RBEBase> value) {
            this.List.Add(new RBEDictionary(value));
        }

        public void AddList(List<RBEBase> value) {
            this.List.Add(new RBEList(value));
        }

        public void AddInt8(byte value) {
            this.List.Add(new RBEByte(value));
        }

        public void AddInt16(short value) {
            this.List.Add(new RBEShort(value));
        }

        public void AddInt32(int value) {
            this.List.Add(new RBEInt(value));
        }

        public void AddInt64(long value) {
            this.List.Add(new RBELong(value));
        }

        public void AddFloat(float value) {
            this.List.Add(new RBEFloat(value));
        }

        public void AddDouble(double value) {
            this.List.Add(new RBEDouble(value));
        }

        public void AddStruct<T>(in T value) where T : unmanaged {
            RBEStruct obj = new RBEStruct();
            obj.SetValue(value);
            this.List.Add(obj);
        }

        public void AddStructArray<T>(in T[] value) where T : unmanaged {
            RBEStructArray obj = new RBEStructArray();
            obj.SetValues(value);
            this.List.Add(obj);
        }

        public override void Read(BinaryReader reader) {
            int length = reader.ReadUInt16();
            this.List = new List<RBEBase>(length);
            for (int i = 0; i < length; i++) {
                this.List.Add(ReadIdAndElement(reader));
            }
        }

        public override void Write(BinaryWriter writer) {
            writer.Write((ushort) this.List.Count);
            foreach (RBEBase child in this.List) {
                WriteIdAndElement(writer, child);
            }
        }

        public override RBEBase CloneCore() => this.Clone();

        public RBEList Clone() {
            List<RBEBase> list = new List<RBEBase>(this.List);
            // not using Select because there's a possibility it causes a stack overflow exception,
            // because there could be a huge chain of elements (lists in lists in lists etc...)
            foreach (RBEBase element in this.List)
                list.Add(element.CloneCore());
            return new RBEList(list);
        }
    }
}