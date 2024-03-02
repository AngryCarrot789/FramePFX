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
    /// An implementation of <see cref="IContextData"/> that stores static entries in an internal dictionary
    /// </summary>
    public class ContextData : IContextData {
        private Dictionary<string, object> map;

        /// <summary>
        /// The number of entries in our internal map
        /// </summary>
        public int Count => this.map?.Count ?? 0;

        public IEnumerable<KeyValuePair<string, object>> Entries => this.map ?? Enumerable.Empty<KeyValuePair<string, object>>();

        /// <summary>
        /// Creates a new empty instance
        /// </summary>
        public ContextData() {
        }

        /// <summary>
        /// Copy constructor, effectively the same as <see cref="Clone"/>
        /// </summary>
        /// <param name="ctx">The context to copy, if non-null</param>
        public ContextData(ContextData ctx) {
            if (ctx.map != null && ctx.map.Count > 0)
                this.map = new Dictionary<string, object>(ctx.map);
        }

        /// <summary>
        /// Copy constructor, effectively the same as <see cref="Clone"/>
        /// </summary>
        /// <param name="ctx">The context to copy, if non-null</param>
        public ContextData(IContextData context) {
            if (context is ContextData ctx) {
                if (ctx.map != null && ctx.map.Count > 0)
                    this.map = new Dictionary<string, object>(ctx.map);
            }
            else if (!(context is EmptyContext)) {
                using (IEnumerator<KeyValuePair<string, object>> enumerator = context.Entries.GetEnumerator()) {
                    if (!enumerator.MoveNext())
                        return;
                    this.map = new Dictionary<string, object>();
                    do {
                        KeyValuePair<string, object> entry = enumerator.Current;
                        this.map[entry.Key] = entry.Value;
                    } while (enumerator.MoveNext());
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

        IContextData IContextData.Clone() => this.Clone();

        /// <summary>
        /// Creates a new instance of <see cref="ContextData"/> containing all entries from this instance
        /// </summary>
        /// <returns>A new cloned instance</returns>
        public ContextData Clone() {
            ContextData ctx = new ContextData();
            if (this.map != null && this.map.Count > 0)
                ctx.map = new Dictionary<string, object>(this.map);
            return ctx;
        }

        public void Merge(IContextData ctx) {
            if (ctx is ContextData cd && cd.map != null) {
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

        public override string ToString() {
            string details = "";
            if (this.map != null && this.map.Count > 0) {
                details = string.Join(", ", this.map.Select(x => "\"" + x.Key + "\"" + "=" + x.Value));
            }

            return "ContextData[" + details + "]";
        }

        /// <summary>
        /// Creates a new context data instance containing the values of dataA, and then merges that with dataB,
        /// ensuring any existing entries in dataA and replaced by the entries in dataB. Always returns a unique
        /// instance and does not modify dataA or dataB
        /// </summary>
        /// <param name="dataA">Source</param>
        /// <param name="dataB">Merge</param>
        /// <returns>A new context data containing entries from dataA and dataB</returns>
        public static ContextData Merge(ContextData dataA, ContextData dataB) {
            ContextData data = new ContextData(dataA);
            data.Merge(dataB);
            return data;
        }
    }
}