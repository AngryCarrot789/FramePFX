using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPadV2.Core.Actions.Contexts {
    public class DefaultDataContext : IDataContext {
        private Dictionary<string, object> data;
        private readonly List<object> context;

        public IEnumerable<object> Context => this.context;

        public IEnumerable<(string, object)> CustomData => this.data != null ? this.data.Select(x => (x.Key, x.Value)) : Enumerable.Empty<(string, object)>();

        public DefaultDataContext() {
            this.context = new List<object>();
        }

        public void AddContext(object context) {
            this.context.Add(context);
        }

        public T GetContext<T>() {
            this.TryGetContext(out T value); // value will be default or null
            return value;
        }

        public bool TryGetContext<T>(out T value) {
            foreach (object obj in this.context) {
                if (obj is T t) {
                    value = t;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public bool TryGet<T>(string key, out T value) {
            return TryGetData(this.data, key, out value);
        }

        public T Get<T>(string key) {
            this.TryGet(key, out T value);
            return value; // ValueType will be default, object will be null
        }

        public void Set(string key, object value) {
            SetData(ref this.data, key, value);
        }

        public void Merge(IDataContext ctx) {
            foreach (object o in ctx.Context) {
                this.context.Add(o);
            }

            foreach ((string a, object b) in ctx.CustomData) {
                this.data[a] = b;
            }
        }

        public static void SetData(ref Dictionary<string, object> map, string key, object value) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key), "Key cannot be null");
            }

            if (value == null) {
                map?.Remove(key);
            }
            else {
                if (map == null) {
                    map = new Dictionary<string, object>();
                }

                map[key] = value;
            }
        }

        public static bool TryGetData<T>(Dictionary<string, object> map, string key, out T value) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key), "Key cannot be null");
            }

            if (map != null && map.TryGetValue(key, out object data) && data is T t) {
                value = t;
                return true;
            }

            value = default;
            return false;
        }
    }
}