using System;
using System.Collections.Generic;
using FramePFX.Utils;

namespace FramePFX.Configurations.Sections
{
    public class MemorySection : ISection
    {
        private readonly MemorySection parent;
        private readonly Dictionary<string, object> map;

        public string Name { get; }

        public ISection Parent { get => this.parent; }

        public MemorySection(MemorySection parent, string name) : this(parent, name, new Dictionary<string, object>())
        {
        }

        public MemorySection(MemorySection parent, string name, Dictionary<string, object> map)
        {
            this.map = map ?? throw new ArgumentNullException(nameof(map), "Map cannot be null");
            this.parent = parent;
            this.Name = name;
        }

        public object this[string key]
        {
            get => this.map.TryGetValue(key ?? "", out object value) ? value : null;
            set
            {
                if (value != null)
                {
                    this.map[key ?? ""] = value;
                }
                else
                {
                    this.map.Remove(key ?? "");
                }
            }
        }

        public object Get(string path)
        {
            return this.WalkPath(path, out string lastKey, false) ? this[lastKey] : null;
        }

        public void Set(string path, object value)
        {
            this.WalkPath(path, out string lastKey, true);
            this[lastKey] = value;
        }

        public bool Set(string path, object value, bool createSubSections = true)
        {
            if (this.WalkPath(path, out string lastKey, createSubSections))
            {
                this[lastKey] = value;
                return true;
            }

            return false;
        }

        public bool TryGet(string path, out object value)
        {
            return (value = this.Get(path)) != null;
        }

        public bool Contains(string path)
        {
            return this.TryGet(path, out object value) && value != null;
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public object Replace(string key, object value)
        {
            object old = this[key];
            this[key] = value;
            return old;
        }

        public int GetInt(string path, int def = 0)
        {
            return this.TryGet(path, out object value) && int.TryParse(value.ToString(), out int val) ? val : def;
        }

        public long GetLong(string path, long def = 0)
        {
            return this.TryGet(path, out object value) && long.TryParse(value.ToString(), out long val) ? val : def;
        }

        public float GetFloat(string path, float def = 0)
        {
            return this.TryGet(path, out object value) && float.TryParse(value.ToString(), out float val) ? val : def;
        }

        public double GetDouble(string path, double def = 0)
        {
            return this.TryGet(path, out object value) && double.TryParse(value.ToString(), out double val) ? val : def;
        }

        public ISection GetSection(string key)
        {
            return this[key] as ISection;
        }

        public ISection GetOrCreateSection(string key)
        {
            return this.GetSection(key) ?? this.CreateSection(key);
        }

        public ISection CreateSection(string key)
        {
            if (key == null)
            {
                key = "";
            }

            ISection section = new MemorySection(this, key);
            this[key] = section;
            return section;
        }

        private bool WalkPath(string path, out string lastKey, bool createSections, char split = '.', char escape = '\\')
        {
            int i, j;
            if (string.IsNullOrEmpty(path) || (i = path.IndexOf(split, j = 0)) == -1)
            {
                lastKey = path ?? "";
                return true;
            }

            ISection section = this;
            FastStringBuf sb = new FastStringBuf(path.Length);
            do
            {
                if (i == 0 || path[i - i] != escape)
                {
                    sb.append(path, j, i);
                    string element = sb.ToString();
                    sb.count = 0;

                    ISection next = createSections ? section.GetOrCreateSection(element) : section.GetSection(element);
                    if (next == null)
                    {
                        lastKey = null;
                        return false;
                    }

                    section = next;
                }
                else
                {
                    sb.append(path, j, i - 1);
                    sb.append(escape);
                }
            } while ((i = path.IndexOf(split, i + 1)) != -1);

            sb.append(path, j);
            lastKey = sb.ToString();
            return true;
        }
    }
}