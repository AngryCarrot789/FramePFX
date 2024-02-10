using System.Collections.Generic;
using System.Linq;
using FramePFX.Editors.Timelines.Clips;

namespace FramePFX.Interactivity.DataContexts {
    public class DataContext : IDataContext {
        private Dictionary<DataKey, object> map;

        public IEnumerable<KeyValuePair<DataKey, object>> Entries => this.map ?? Enumerable.Empty<KeyValuePair<DataKey, object>>();

        public DataContext() {

        }

        public DataContext(IDataContext ctx) {
            if (ctx is DataContext dctx) {
                this.map = dctx.map != null ? new Dictionary<DataKey, object>(dctx.map) : null;
            }
            else if (ctx != EmptyContext.Instance) {
                using (IEnumerator<KeyValuePair<DataKey, object>> enumerable = ctx.Entries.GetEnumerator()) {
                    if (enumerable.MoveNext()) {
                        this.map = new Dictionary<DataKey, object>();
                        do {
                            KeyValuePair<DataKey, object> entry = enumerable.Current;
                            this.map[entry.Key] = entry.Value;
                        } while (enumerable.MoveNext());
                    }
                }
            }
        }

        public DataContext Set<T>(DataKey<T> key, T value) => this.SetRaw(key, value);

        public DataContext SetRaw(DataKey key, object value) {
            if (value == null) {
                this.map?.Remove(key);
            }
            else {
                (this.map ?? (this.map = new Dictionary<DataKey, object>()))[key] = value;
            }

            return this;
        }

        public bool TryGetContext<T>(DataKey<T> key, out T value) {
            if (this.map != null && this.map.TryGetValue(key, out object objVal)) {
                value = (T) objVal;
                return true;
            }

            value = default;
            return false;
        }

        public bool ContainsKey(DataKey key) {
            return this.map != null && this.map.ContainsKey(key);
        }

        public bool HasFlag(DataKey<bool> key) {
            return this.map != null && this.map.TryGetValue(key, out object objValue) && (bool) objValue;
        }

        public DataContext Clone() {
            DataContext ctx = new DataContext();
            if (this.map != null)
                ctx.map = new Dictionary<DataKey, object>(this.map);
            return ctx;
        }

        public void Merge(IDataContext ctx) {
            if (ctx is DataContext dc) {
                if (dc.map != null) {
                    using (Dictionary<DataKey, object>.Enumerator enumerator = dc.map.GetEnumerator()) {
                        if (enumerator.MoveNext()) {
                            Dictionary<DataKey, object> myMap = this.map ?? (this.map = new Dictionary<DataKey, object>());
                            do {
                                KeyValuePair<DataKey, object> entry = enumerator.Current;
                                myMap[entry.Key] = entry.Value;
                            } while (enumerator.MoveNext());
                        }
                    }
                }
            }
            else if (ctx != null && !(ctx is EmptyContext)) {
                using (IEnumerator<KeyValuePair<DataKey, object>> enumerator = ctx.Entries.GetEnumerator()) {
                    // try not to allocate map when there are no entries
                    if (enumerator.MoveNext()) {
                        Dictionary<DataKey, object> myMap = this.map ?? (this.map = new Dictionary<DataKey, object>());
                        do {
                            KeyValuePair<DataKey, object> entry = enumerator.Current;
                            myMap[entry.Key] = entry.Value;
                        } while (enumerator.MoveNext());
                    }
                }
            }
        }

        public override string ToString() {
            string details = "";
            if (this.map != null && this.map.Count > 0) {
                details = string.Join(", ", this.map.Select(x => "\"" + x.Key.ReadableName + "\"" + "=" + x.Value));
            }

            return "$DataContext[" + details + "]";
        }
    }
}