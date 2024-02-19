//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System.Collections.Generic;
using System.Linq;
using FramePFX.Utils;

namespace FramePFX.Interactivity.Contexts {
    /// <summary>
    /// The main implementation of <see cref="IContextData"/>
    /// </summary>
    public class ContextData : IContextData {
        private Dictionary<string, object> map;

        public IEnumerable<KeyValuePair<string, object>> Entries => this.map ?? Enumerable.Empty<KeyValuePair<string, object>>();

        public int Count => this.map?.Count ?? 0;

        public ContextData() {

        }

        public ContextData(IContextData ctx) {
            if (ctx is ContextData cd) {
                this.map = cd.map != null ? new Dictionary<string, object>(cd.map) : null;
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

        public ContextData Set<T>(DataKey<T> key, T value) => this.SetRaw(key.Id, value);
        public ContextData Set(DataKey<bool> key, bool? value) => this.SetRaw(key.Id, value.BoxNullable());

        public ContextData SetRaw(string key, object value) {
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

        public ContextData Clone() {
            ContextData ctx = new ContextData();
            if (this.map != null)
                ctx.map = new Dictionary<string, object>(this.map);
            return ctx;
        }

        public void Merge(IContextData ctx) {
            if (ctx is ContextData cd) {
                if (cd.map != null) {
                    using (Dictionary<string, object>.Enumerator enumerator = cd.map.GetEnumerator()) {
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

            return "ContextData[" + details + "]";
        }
    }
}