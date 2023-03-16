using System;
using System.Collections.Generic;
using System.IO;

namespace FramePFX.Core.RBC {
    public class RBEDictionary : RBEBase {
        public Dictionary<string, RBEBase> Map { get; private set; }

        public override int TypeId => 1;

        public RBEDictionary() : this(new Dictionary<string, RBEBase>()) {

        }

        public RBEDictionary(Dictionary<string, RBEBase> map) {
            this.Map = map ?? throw new ArgumentNullException(nameof(map), "Map cannot be null");
        }

        public RBEBase this[string key] {
            get => this.Map.TryGetValue(key ?? "", out RBEBase value) ? value : null;
            set {
                if (value != null) {
                    this.Map[key ?? ""] = value;
                }
                else {
                    this.Map.Remove(key ?? "");
                }
            }
        }

        public bool TryGet(string key, out RBEBase value) {
            return (value = this[key]) != null;
        }

        private bool GetElementByType<T>(string key, out T value) where T : RBEBase {
            if (this.Map.TryGetValue(key ?? "", out RBEBase rbe)) {
                if (rbe is T val) {
                    value = val;
                    return true;
                }
                else {
                    throw new Exception($"Incompatible types: Attempted to get element named {key} of type {typeof(T)}, but type {rbe?.GetType()} was found");
                }
            }
            else {
                value = default;
                return false;
            }
        }

        public RBEDictionary GetDictionaryElement(string key) {
            return this.GetElementByType(key, out RBEDictionary rbe) ? rbe : null;
        }

        public Dictionary<string, RBEBase> GetDictionary(string key, Dictionary<string, RBEBase> def = default) {
            return this.GetElementByType(key, out RBEDictionary rbe) ? rbe.Map : def;
        }

        public bool TryGetDictionary(string key, out Dictionary<string, RBEBase> value) {
            RBEDictionary element = this.GetDictionaryElement(key);
            value = element?.Map;
            return element != null;
        }

        public RBEList GetListElement(string key) {
            return this.GetElementByType(key, out RBEList rbe) ? rbe : null;
        }

        public List<RBEBase> GetList(string key, List<RBEBase> def = default) {
            return this.GetElementByType(key, out RBEList rbe) ? rbe.List : def;
        }

        public bool TryGetList(string key, out List<RBEBase> value) {
            RBEList element = this.GetListElement(key);
            value = element?.List;
            return element != null;
        }

        public RBEInt8 GetInt8Element(string key) {
            return this.GetElementByType(key, out RBEInt8 rbe) ? rbe : null;
        }

        public byte GetInt8(string key, byte def = default) {
            return this.GetElementByType(key, out RBEInt8 rbe) ? rbe.Value : def;
        }

        public bool TryGetInt8(string key, out byte value) {
            RBEInt8 element = this.GetInt8Element(key);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBEInt16 GetInt16Element(string key) {
            return this.GetElementByType(key, out RBEInt16 rbe) ? rbe : null;
        }

        public short GetInt16(string key, short def = default) {
            return this.GetElementByType(key, out RBEInt16 rbe) ? rbe.Value : def;
        }

        public bool TryGetInt16(string key, out short value) {
            RBEInt16 element = this.GetInt16Element(key);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBEInt32 GetInt32Element(string key) {
            return this.GetElementByType(key, out RBEInt32 rbe) ? rbe : null;
        }

        public int GetInt32(string key, int def = default) {
            return this.GetElementByType(key, out RBEInt32 rbe) ? rbe.Value : def;
        }

        public bool TryGetInt32(string key, out int value) {
            RBEInt32 element = this.GetInt32Element(key);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBEInt64 GetInt64Element(string key) {
            return this.GetElementByType(key, out RBEInt64 rbe) ? rbe : null;
        }

        public long GetInt64(string key, long def = default) {
            return this.GetElementByType(key, out RBEInt64 rbe) ? rbe.Value : def;
        }

        public bool TryGetInt64(string key, out long value) {
            RBEInt64 element = this.GetInt64Element(key);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBEFloat GetFloatElement(string key) {
            return this.GetElementByType(key, out RBEFloat rbe) ? rbe : null;
        }

        public float GetFloat(string key, float def = default) {
            return this.GetElementByType(key, out RBEFloat rbe) ? rbe.Value : def;
        }

        public bool TryGetFloat(string key, out float value) {
            RBEFloat element = this.GetFloatElement(key);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBEDouble GetDoubleElement(string key) {
            return this.GetElementByType(key, out RBEDouble rbe) ? rbe : null;
        }

        public double GetDouble(string key, double def = default) {
            return this.GetElementByType(key, out RBEDouble rbe) ? rbe.Value : def;
        }

        public bool TryGetDouble(string key, out double value) {
            RBEDouble element = this.GetDoubleElement(key);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBEStruct GetStructElement(string key) {
            return this.GetElementByType(key, out RBEStruct rbe) ? rbe : null;
        }

        public T GetStruct<T>(string key, T def = default) where T : unmanaged {
            return this.GetElementByType(key, out RBEStruct value) ? value.GetValue<T>() : def;
        }

        public bool TryGetStruct<T>(string key, out T value) where T : unmanaged {
            RBEStruct element = this.GetStructElement(key);
            value = element?.GetValue<T>() ?? default;
            return element != null;
        }

        public void SetDictionary(string key, Dictionary<string, RBEBase> value) {
            this[key] = new RBEDictionary(value);
        }

        public void SetList(string key, List<RBEBase> value) {
            this[key] = new RBEList(value);
        }

        public void SetInt8(string key, byte value) {
            this[key] = new RBEInt8(value);
        }

        public void SetInt16(string key, short value) {
            this[key] = new RBEInt16(value);
        }

        public void SetInt32(string key, int value) {
            this[key] = new RBEInt32(value);
        }

        public void SetInt64(string key, long value) {
            this[key] = new RBEInt64(value);
        }

        public void SetFloat(string key, float value) {
            this[key] = new RBEFloat(value);
        }

        public void SetDouble(string key, double value) {
            this[key] = new RBEDouble(value);
        }

        public void SetStruct<T>(string key, in T value) where T : unmanaged {
            RBEStruct obj = new RBEStruct();
            this[key] = obj;
            obj.SetValue(value);
        }

        public override void Read(BinaryReader reader) {
            int length = reader.ReadUInt16();
            this.Map = new Dictionary<string, RBEBase>(length);
            for (int i = 0; i < length; i++) {
                string key = new string(reader.ReadChars(reader.ReadByte()));
                RBEBase element = ReadIdAndElement(reader);
                this.Map[key] = element;
            }
        }

        public override void Write(BinaryWriter writer) {
            writer.Write((ushort) this.Map.Count);
            foreach (KeyValuePair<string, RBEBase> entry in this.Map) {
                writer.Write((byte) entry.Key.Length);
                writer.Write(entry.Key.ToCharArray());
                WriteIdAndElement(writer, entry.Value);
            }
        }
    }
}