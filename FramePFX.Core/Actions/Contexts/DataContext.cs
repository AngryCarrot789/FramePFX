using System;
using System.Collections.Generic;
using System.Linq;

namespace FramePFX.Core.Actions.Contexts {
    public class DataContext : IDataContext {
        public Dictionary<string, object> EntryMap { get; set; }
        public List<object> InternalContext { get; }

        public IEnumerable<object> Context => this.InternalContext;

        // not read only dictionary because EntryMap may be null
        public IEnumerable<(string, object)> Entries => this.EntryMap != null ? this.EntryMap.Select(x => (x.Key, x.Value)) : Enumerable.Empty<(string, object)>();

        public DataContext() {
            this.InternalContext = new List<object>();
        }

        public DataContext(object primaryContext) : this() {
            this.AddContext(primaryContext);
        }

        public T GetContext<T>() {
            this.TryGetContext(out T value); // value will be default or null
            return value;
        }

        public bool TryGetContext<T>(out T value) {
            foreach (object obj in this.InternalContext) {
                if (obj is T t) {
                    value = t;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public bool TryGetContext(Type type, out object value) {
            return (value = this.InternalContext.First(type.IsInstanceOfType)) != null;
        }

        public bool HasContext<T>() {
            return this.InternalContext.Any(x => x is T);
        }

        public bool TryGet<T>(string key, out T value) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key), "Key cannot be null");
            }

            if (this.EntryMap != null && this.EntryMap.TryGetValue(key, out object data) && data is T t) {
                value = t;
                return true;
            }

            value = default;
            return false;
        }

        public bool ContainsKey(string key) {
            return this.TryGet<object>(key, out _);
        }

        public bool HasFlag(string key) {
            return this.TryGet(key, out bool value) && value;
        }

        public T Get<T>(string key) {
            this.TryGet(key, out T value);
            return value; // ValueType will be default, object will be null
        }

        public void AddContext(object context) {
            this.InternalContext.Add(context);
        }

        public void Set(string key, object value) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key), "Key cannot be null");
            }

            if (value == null) {
                this.EntryMap?.Remove(key);
            }
            else {
                if (this.EntryMap == null) {
                    this.EntryMap = new Dictionary<string, object>();
                }

                this.EntryMap[key] = value;
            }
        }

        public void Merge(IDataContext ctx) {
            foreach (object value in ctx.Context) {
                this.InternalContext.Add(value);
            }

            if (ctx is DataContext ctxImpl) {
                // slight optimisation; no need to deconstruct KeyValuePairs into tuples
                if (ctxImpl.EntryMap != null && ctxImpl.EntryMap.Count > 0) {
                    if (this.EntryMap == null) {
                        this.EntryMap = new Dictionary<string, object>(ctxImpl.EntryMap);
                    }
                    else {
                        foreach (KeyValuePair<string, object> entry in ctxImpl.EntryMap) {
                            this.EntryMap[entry.Key] = entry.Value;
                        }
                    }
                }
            }
            else {
                List<(string, object)> list = ctx.Entries.ToList();
                if (list.Count < 1) {
                    return;
                }

                Dictionary<string, object> map = this.EntryMap ?? (this.EntryMap = new Dictionary<string, object>());
                foreach ((string a, object b) in list) {
                    map[a] = b;
                }
            }
        }
    }
}