using System;
using System.Collections.Generic;
using System.IO;

namespace FramePFX.Core.RBC {
    public class RBEList : RBEBase {
        public List<RBEBase> List { get; private set; }

        public override int TypeId => 2;

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

        public Dictionary<string, RBEBase> GetDictionary(int index, Dictionary<string, RBEBase> def = default) {
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

        public List<RBEBase> GetList(int index, List<RBEBase> def = default) {
            return this.GetElementByType(index, out RBEList rbe) ? rbe.List : def;
        }

        public bool TryGetList(int index, out List<RBEBase> value) {
            RBEList element = this.GetListElement(index);
            value = element?.List;
            return element != null;
        }

        public RBEInt8 GetInt8Element(int index) {
            return this.GetElementByType(index, out RBEInt8 rbe) ? rbe : null;
        }

        public byte GetInt8(int index, byte def = default) {
            return this.GetElementByType(index, out RBEInt8 rbe) ? rbe.Value : def;
        }

        public bool TryGetInt8(int index, out byte value) {
            RBEInt8 element = this.GetInt8Element(index);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBEInt16 GetInt16Element(int index) {
            return this.GetElementByType(index, out RBEInt16 rbe) ? rbe : null;
        }

        public short GetInt16(int index, short def = default) {
            return this.GetElementByType(index, out RBEInt16 rbe) ? rbe.Value : def;
        }

        public bool TryGetInt16(int index, out short value) {
            RBEInt16 element = this.GetInt16Element(index);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBEInt32 GetInt32Element(int index) {
            return this.GetElementByType(index, out RBEInt32 rbe) ? rbe : null;
        }

        public int GetInt32(int index, int def = default) {
            return this.GetElementByType(index, out RBEInt32 rbe) ? rbe.Value : def;
        }

        public bool TryGetInt32(int index, out int value) {
            RBEInt32 element = this.GetInt32Element(index);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBEInt64 GetInt64Element(int index) {
            return this.GetElementByType(index, out RBEInt64 rbe) ? rbe : null;
        }

        public long GetInt64(int index, long def = default) {
            return this.GetElementByType(index, out RBEInt64 rbe) ? rbe.Value : def;
        }

        public bool TryGetInt64(int index, out long value) {
            RBEInt64 element = this.GetInt64Element(index);
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

        public T GetStruct<T>(int index, T def = default) where T : unmanaged {
            return this.GetElementByType(index, out RBEStruct value) ? value.GetValue<T>() : def;
        }

        public bool TryGetStruct<T>(int index, out T value) where T : unmanaged {
            RBEStruct element = this.GetStructElement(index);
            value = element?.GetValue<T>() ?? default;
            return element != null;
        }
        
        public void AddDictionary(Dictionary<string, RBEBase> value) {
            this.List.Add(new RBEDictionary(value));
        }

        public void AddList(List<RBEBase> value) {
            this.List.Add(new RBEList(value));
        }

        public void AddInt8(byte value) {
            this.List.Add(new RBEInt8(value));
        }

        public void AddInt16(short value) {
            this.List.Add(new RBEInt16(value));
        }

        public void AddInt32(int value) {
            this.List.Add(new RBEInt32(value));
        }

        public void AddInt64(long value) {
            this.List.Add(new RBEInt64(value));
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
    }
}