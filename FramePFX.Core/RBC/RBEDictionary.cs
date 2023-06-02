using System;
using System.Collections.Generic;
using System.IO;

namespace FrameControlEx.Core.RBC {
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

        public T GetElement<T>(string key) where T : RBEBase {
            if (this.TryGetElement(key, out T value)) {
                return value;
            }
            else {
                string readableTypeName = TryGetIdByType(typeof(T), out var type) ? type.ToString() : typeof(T).ToString();
                throw new Exception($"No such {readableTypeName} named '{key}'");
            }
        }

        public bool TryGetElement<T>(string key, out T element) where T : RBEBase {
            if (this.Map.TryGetValue(ValidateKey(key), out RBEBase rbeBase)) {
                if (rbeBase is T rbe) {
                    element = rbe;
                    return true;
                }
            }

            element = default;
            return false;
        }

        public bool TryGetElementValue<TElement, T>(string key, Func<TElement, T> elemToVal, out T value) where TElement : RBEBase {
            if (this.TryGetElement(key, out TElement element)) {
                value = elemToVal(element);
                return true;
            }

            value = default;
            return false;
        }

        public Dictionary<string, RBEBase> GetDictionary(string key) => this.GetElement<RBEDictionary>(key).Map;
        public Dictionary<string, RBEBase> GetDictionary(string key, Dictionary<string, RBEBase> def) => this.TryGetElement(key, out RBEDictionary rbe) ? rbe.Map : def;
        public bool TryGetDictionary(string key, out Dictionary<string, RBEBase> value) => (value = this.GetDictionary(key)) != null;
        public RBEDictionary GetOrCreateDictionaryElement(string key) {
            if (!this.TryGetElement(key, out RBEDictionary dictionary))
                this[key] = dictionary = new RBEDictionary();
            return dictionary;
        }

        public Dictionary<string, RBEBase> GetOrCreateDictionary(string key) => this.GetOrCreateDictionaryElement(key).Map;

        public List<RBEBase> GetList(string key) => this.GetElement<RBEList>(key).List;
        public List<RBEBase> GetList(string key, List<RBEBase> def) => this.TryGetElement(key, out RBEList rbe) ? rbe.List : def;
        public bool TryGetList(string key, out List<RBEBase> value) => (value = this.GetList(key)) != null;
        public RBEList GetOrCreateListElement(string key) {
            if (!this.TryGetElement(key, out RBEList dictionary))
                this[key] = dictionary = new RBEList();
            return dictionary;
        }

        public List<RBEBase> GetOrCreateList(string key) => this.GetOrCreateListElement(key).List;

        public bool GetBool(string key) => this.GetByte(key) != 0;
        public bool GetBool(string key, bool def) => this.TryGetElement(key, out RBEByte rbe) ? (rbe.Value != 0) : def;
        public bool TryGetBool(string key, out bool value) => this.TryGetElementValue<RBEByte, bool>(key, (e) => e.Value != 0, out value);

        public byte GetByte(string key) => this.GetElement<RBEByte>(key).Value;
        public byte GetByte(string key, byte def) => this.TryGetElement(key, out RBEByte rbe) ? rbe.Value : def;
        public bool TryGetByte(string key, out byte value) => this.TryGetElementValue<RBEByte, byte>(key, (e) => e.Value, out value);

        public short GetShort(string key) => this.GetElement<RBEShort>(key).Value;
        public short GetShort(string key, short def) => this.TryGetElement(key, out RBEShort rbe) ? rbe.Value : def;
        public bool TryGetShort(string key, out short value) => this.TryGetElementValue<RBEShort, short>(key, (e) => e.Value, out value);

        public int GetInt(string key) => this.GetElement<RBEInt>(key).Value;
        public int GetInt(string key, int def) => this.TryGetElement(key, out RBEInt rbe) ? rbe.Value : def;
        public bool TryGetInt(string key, out int value) => this.TryGetElementValue<RBEInt, int>(key, (e) => e.Value, out value);

        public long GetLong(string key) => this.GetElement<RBELong>(key).Value;
        public long GetLong(string key, long def) => this.TryGetElement(key, out RBELong rbe) ? rbe.Value : def;
        public bool TryGetLong(string key, out long value) => this.TryGetElementValue<RBELong, long>(key, (e) => e.Value, out value);

        public float GetFloat(string key) => this.GetElement<RBEFloat>(key).Value;
        public float GetFloat(string key, float def) => this.TryGetElement(key, out RBEFloat rbe) ? rbe.Value : def;
        public bool TryGetFloat(string key, out float value) => this.TryGetElementValue<RBEFloat, float>(key, (e) => e.Value, out value);

        public double GetDouble(string key) => this.GetElement<RBEDouble>(key).Value;
        public double GetDouble(string key, double def) => this.TryGetElement(key, out RBEDouble rbe) ? rbe.Value : def;
        public bool TryGetDouble(string key, out double value) => this.TryGetElementValue<RBEDouble, double>(key, (e) => e.Value, out value);

        public string GetString(string key) => this.GetElement<RBEString>(key).Value;
        public string GetString(string key, string def) => this.TryGetElement(key, out RBEString rbe) ? rbe.Value : def;
        public bool TryGetString(string key, out string value) => this.TryGetElementValue<RBEString, string>(key, (e) => e.Value, out value);

        public T GetStruct<T>(string key) where T : unmanaged => this.GetElement<RBEStruct>(key).GetValue<T>();
        public T GetStruct<T>(string key, T def) where T : unmanaged => this.TryGetElement(key, out RBEStruct rbe) ? rbe.GetValue<T>() : def;

        public bool TryGetStruct<T>(string key, out T value) where T : unmanaged {
            if (this.TryGetElement(key, out RBEStruct rbe) && rbe.TryGetValue(out value)) {
                return true;
            }

            value = default;
            return false;
        }

        public byte[] GetByteArray(string key) => this.GetElement<RBEByteArray>(key).Array;
        public byte[] GetByteArray(string key, byte[] def) => this.TryGetElement(key, out RBEByteArray rbe) ? rbe.Array : def;
        public bool TryGetByteArray(string key, out byte[] value) => this.TryGetElementValue<RBEByteArray, byte[]>(key, (e) => e.Array, out value);

        public short[] GetShortArray(string key) => this.GetElement<RBEShortArray>(key).Array;
        public short[] GetShortArray(string key, short[] def) => this.TryGetElement(key, out RBEShortArray rbe) ? rbe.Array : def;
        public bool TryGetShortArray(string key, out short[] value) => this.TryGetElementValue<RBEShortArray, short[]>(key, (e) => e.Array, out value);

        public int[] GetIntArray(string key) => this.GetElement<RBEIntArray>(key).Array;
        public int[] GetIntArray(string key, int[] def) => this.TryGetElement(key, out RBEIntArray rbe) ? rbe.Array : def;
        public bool TryGetIntArray(string key, out int[] value) => this.TryGetElementValue<RBEIntArray, int[]>(key, (e) => e.Array, out value);

        public long[] GetLongArray(string key) => this.GetElement<RBELongArray>(key).Array;
        public long[] GetLongArray(string key, long[] def) => this.TryGetElement(key, out RBELongArray rbe) ? rbe.Array : def;
        public bool TryGetLongArray(string key, out long[] value) => this.TryGetElementValue<RBELongArray, long[]>(key, (e) => e.Array, out value);

        public float[] GetFloatArray(string key) => this.GetElement<RBEFloatArray>(key).Array;
        public float[] GetFloatArray(string key, float[] def) => this.TryGetElement(key, out RBEFloatArray rbe) ? rbe.Array : def;
        public bool TryGetFloatArray(string key, out float[] value) => this.TryGetElementValue<RBEFloatArray, float[]>(key, (e) => e.Array, out value);

        public double[] GetDoubleArray(string key) => this.GetElement<RBEDoubleArray>(key).Array;
        public double[] GetDoubleArray(string key, double[] def) => this.TryGetElement(key, out RBEDoubleArray rbe) ? rbe.Array : def;
        public bool TryGetDoubleArray(string key, out double[] value) => this.TryGetElementValue<RBEDoubleArray, double[]>(key, (e) => e.Array, out value);

        public string[] GetStringArray(string key) => this.GetElement<RBEStringArray>(key).Array;
        public string[] GetStringArray(string key, string[] def) => this.TryGetElement(key, out RBEStringArray rbe) ? rbe.Array : def;
        public bool TryGetStringArray(string key, out string[] value) => this.TryGetElementValue<RBEStringArray, string[]>(key, (e) => e.Array, out value);

        public T[] GetStructArray<T>(string key) where T : unmanaged => this.GetElement<RBEStructArray>(key).GetValues<T>();
        public T[] GetStructArray<T>(string key, T[] def) where T : unmanaged => this.TryGetElement(key, out RBEStructArray rbe) ? rbe.GetValues<T>() : def;
        public bool TryGetStructArray<T>(string key, out T[] value) where T : unmanaged {
            if (this.TryGetElement(key, out RBEStructArray rbe) && rbe.TryGetValues(out value)) {
                return true;
            }

            value = default;
            return false;
        }

        public void SetDictionary(string key, Dictionary<string, RBEBase> value) => this[key] = new RBEDictionary(value);
        public void SetList(string key, List<RBEBase> value) => this[key] = new RBEList(value);
        public void SetBool(string key, bool value) => this[key] = new RBEByte((byte) (value ? 1 : 0));
        public void SetByte(string key, byte value) => this[key] = new RBEByte(value);
        public void SetShort(string key, short value) => this[key] = new RBEShort(value);
        public void SetInt(string key, int value) => this[key] = new RBEInt(value);
        public void SetLong(string key, long value) => this[key] = new RBELong(value);
        public void SetFloat(string key, float value) => this[key] = new RBEFloat(value);
        public void SetDouble(string key, double value) => this[key] = new RBEDouble(value);
        public void SetString(string key, string value) => this[key] = new RBEString(value);
        public void SetStruct<T>(string key, in T value) where T : unmanaged => this[key] = RBEStruct.ForValue(in value);
        public void SetByteArray(string key, byte[] array) => this[key] = new RBEByteArray(array);
        public void SetShortArray(string key, short[] array) => this[key] = new RBEShortArray(array);
        public void SetIntArray(string key, int[] array) => this[key] = new RBEIntArray(array);
        public void SetLongArray(string key, long[] array) => this[key] = new RBELongArray(array);
        public void SetFloatArray(string key, float[] array) => this[key] = new RBEFloatArray(array);
        public void SetDoubleArray(string key, double[] array) => this[key] = new RBEDoubleArray(array);
        public void SetStringArray(string key, string[] array) => this[key] = new RBEStringArray(array);
        public void SetStructArray<T>(string key, T[] array) where T : unmanaged => this[key] = RBEStructArray.ForValues(array);

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

        public override RBEBase Clone() => this.CloneCore();

        public RBEDictionary CloneCore() {
            Dictionary<string, RBEBase> map = new Dictionary<string, RBEBase>(this.Map.Count);
            foreach (KeyValuePair<string, RBEBase> element in this.Map)
                map[element.Key] = element.Value.Clone();
            return new RBEDictionary(map);
        }
    }
}