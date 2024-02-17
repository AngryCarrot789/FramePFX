using System.Collections.Generic;
using System.Linq;
using FramePFX.Utils;

namespace FramePFX.Interactivity.DataContexts {
    public class DataContext : IDataContext {
        private Dictionary<string, object> map;

        public IEnumerable<KeyValuePair<string, object>> Entries => this.map ?? Enumerable.Empty<KeyValuePair<string, object>>();

        public DataContext() {

        }

        public DataContext(IDataContext ctx) {
            if (ctx is DataContext dctx) {
                this.map = dctx.map != null ? new Dictionary<string, object>(dctx.map) : null;
            }
            else if (ctx != EmptyContext.Instance) {
                using (IEnumerator<KeyValuePair<string, object>> enumerable = ctx.Entries.GetEnumerator()) {
                    if (enumerable.MoveNext()) {
                        this.map = new Dictionary<string, object>();
                        do {
                            KeyValuePair<string, object> entry = enumerable.Current;
                            this.map[entry.Key] = entry.Value;
                        } while (enumerable.MoveNext());
                    }
                }
            }
        }

        public DataContext Set<T>(DataKey<T> key, T value) => this.SetRaw(key.Id, value);
        public DataContext Set(DataKey<bool> key, bool? value) => this.SetRaw(key.Id, value.BoxNullable());

        public DataContext SetRaw(string key, object value) {
            if (value == null) {
                this.map?.Remove(key);
            }
            else {
                (this.map ?? (this.map = new Dictionary<string, object>()))[key] = value;
            }

            return this;
        }

        public bool TryGetContext(string key, out object value) {
            if (this.map != null && this.map.TryGetValue(key, out value))
                return true;
            value = default;
            return false;
        }

        public bool ContainsKey(DataKey key) {
            return this.map != null && this.map.ContainsKey(key.Id);
        }

        public bool ContainsKey(string key) {
            return this.map != null && this.map.ContainsKey(key);
        }

        public DataContext Clone() {
            DataContext ctx = new DataContext();
            if (this.map != null)
                ctx.map = new Dictionary<string, object>(this.map);
            return ctx;
        }

        public void Merge(IDataContext ctx) {
            if (ctx is DataContext dc) {
                if (dc.map != null) {
                    using (Dictionary<string, object>.Enumerator enumerator = dc.map.GetEnumerator()) {
                        if (enumerator.MoveNext()) {
                            Dictionary<string, object> myMap = this.map ?? (this.map = new Dictionary<string, object>());
                            do {
                                KeyValuePair<string, object> entry = enumerator.Current;
                                myMap[entry.Key] = entry.Value;
                            } while (enumerator.MoveNext());
                        }
                    }
                }
            }
            else if (ctx != null && !(ctx is EmptyContext)) {
                using (IEnumerator<KeyValuePair<string, object>> enumerator = ctx.Entries.GetEnumerator()) {
                    // try not to allocate map when there are no entries
                    if (enumerator.MoveNext()) {
                        Dictionary<string, object> myMap = this.map ?? (this.map = new Dictionary<string, object>());
                        do {
                            KeyValuePair<string, object> entry = enumerator.Current;
                            myMap[entry.Key] = entry.Value;
                        } while (enumerator.MoveNext());
                    }
                }
            }
        }

        public override string ToString() {
            string details = "";
            if (this.map != null && this.map.Count > 0) {
                details = string.Join(", ", this.map.Select(x => "\"" + x.Key + "\"" + "=" + x.Value));
            }

            return "$DataContext[" + details + "]";
        }
    }
}