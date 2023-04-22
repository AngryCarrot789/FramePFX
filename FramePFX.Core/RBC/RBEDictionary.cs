using System;
using System.Collections.Generic;
using System.IO;

namespace FramePFX.Core.RBC {
    /// <summary>
    /// Used to store named elements using a dictionary
    /// </summary>
    public class RBEDictionary : RBEBase {
        public Dictionary<string, RBEBase> Map { get; private set; }

        public override RBEType Type => RBEType.Dictionary;

        public RBEDictionary() : this(new Dictionary<string, RBEBase>()) {

        }

        public RBEDictionary(Dictionary<string, RBEBase> map) {
            this.Map = map ?? throw new ArgumentNullException(nameof(map), "Map cannot be null");
        }

        public RBEBase this[string key] {
            get {
                return this.Map.TryGetValue(ValidateKey(key), out RBEBase value) ? value : null;
            }
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

        public bool TryGet(string key, out RBEBase value) {
            return (value = this[key]) != null;
        }

        private bool GetElementByType<T>(string key, out T value) where T : RBEBase {
            key = ValidateKey(key);
            if (this.Map.TryGetValue(key, out RBEBase rbe)) {
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

        public Dictionary<string, RBEBase> GetDictionary(string key, Dictionary<string, RBEBase> def = null) {
            return this.GetElementByType(key, out RBEDictionary rbe) ? rbe.Map : def;
        }

        public bool TryGetDictionary(string key, out Dictionary<string, RBEBase> value) {
            RBEDictionary element = this.GetDictionaryElement(key);
            value = element?.Map;
            return element != null;
        }

        public RBEDictionary GetOrCreateDictionary(string key) {
            RBEDictionary element = this.GetDictionaryElement(key);
            if (element == null)
                this[key] = element = new RBEDictionary();
            return element;
        }

        public RBEList GetListElement(string key) {
            return this.GetElementByType(key, out RBEList rbe) ? rbe : null;
        }

        public List<RBEBase> GetList(string key, List<RBEBase> def = null) {
            return this.GetElementByType(key, out RBEList rbe) ? rbe.List : def;
        }

        public bool TryGetList(string key, out List<RBEBase> value) {
            RBEList element = this.GetListElement(key);
            value = element?.List;
            return element != null;
        }

        public RBEByte GetByteElement(string key) {
            return this.GetElementByType(key, out RBEByte rbe) ? rbe : null;
        }

        public byte GetByte(string key, byte def = default) {
            return this.GetElementByType(key, out RBEByte rbe) ? rbe.Value : def;
        }

        public bool TryGetByte(string key, out byte value) {
            RBEByte element = this.GetByteElement(key);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBEShort GetShortElement(string key) {
            return this.GetElementByType(key, out RBEShort rbe) ? rbe : null;
        }

        public short GetShort(string key, short def = default) {
            return this.GetElementByType(key, out RBEShort rbe) ? rbe.Value : def;
        }

        public bool TryGetShort(string key, out short value) {
            RBEShort element = this.GetShortElement(key);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBEInt GetIntElement(string key) {
            return this.GetElementByType(key, out RBEInt rbe) ? rbe : null;
        }

        public int GetInt(string key, int def = default) {
            return this.GetElementByType(key, out RBEInt rbe) ? rbe.Value : def;
        }

        public bool TryGetInt(string key, out int value) {
            RBEInt element = this.GetIntElement(key);
            value = element?.Value ?? default;
            return element != null;
        }

        public RBELong GetLongElement(string key) {
            return this.GetElementByType(key, out RBELong rbe) ? rbe : null;
        }

        public long GetLong(string key, long def = default) {
            return this.GetElementByType(key, out RBELong rbe) ? rbe.Value : def;
        }

        public bool TryGetLong(string key, out long value) {
            RBELong element = this.GetLongElement(key);
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

        public T GetStruct<T>(string key) where T : unmanaged {
            return this.GetElementByType(key, out RBEStruct value) ? value.GetValue<T>() : default;
        }

        public T GetStruct<T>(string key, T def) where T : unmanaged {
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

        public void SetByte(string key, byte value) {
            this[key] = new RBEByte(value);
        }

        public void SetShort(string key, short value) {
            this[key] = new RBEShort(value);
        }

        public void SetInt(string key, int value) {
            this[key] = new RBEInt(value);
        }

        public void SetLong(string key, long value) {
            this[key] = new RBELong(value);
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
                int length = entry.Key.Length;
                if (length > 255) {
                    throw new Exception($"Map contained a key longer than 255 characters: {length}");
                }

                writer.Write((byte) length);
                writer.Write(entry.Key.ToCharArray());
                WriteIdAndElement(writer, entry.Value);
            }
        }

        public static string ValidateKey(string key) {
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

        public override RBEBase CloneCore() => this.Clone();

        public RBEDictionary Clone() {
            Dictionary<string, RBEBase> map = new Dictionary<string, RBEBase>(this.Map.Count);
            foreach (KeyValuePair<string, RBEBase> element in this.Map)
                map[element.Key] = element.Value.CloneCore();
            return new RBEDictionary(map);
        }
    }
}