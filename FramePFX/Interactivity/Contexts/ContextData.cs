// 
// Copyright (c) 2024-2024 REghZy
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

using FramePFX.Utils;

namespace FramePFX.Interactivity.Contexts;

/// <summary>
/// An implementation of <see cref="IContextData"/> that stores static entries in an internal dictionary
/// </summary>
public class ContextData : IRandomAccessContext {
    private Dictionary<string, object>? map;

    /// <summary>
    /// The number of entries in our internal map
    /// </summary>
    public int Count => this.map?.Count ?? 0;

    public IEnumerable<KeyValuePair<string, object>> Entries => this.map ?? EmptyContext.EmptyDictionary;

    /// <summary>
    /// Creates a new empty instance
    /// </summary>
    public ContextData() { }

    /// <summary>
    /// Copy constructor, effectively the same as <see cref="Clone"/>
    /// </summary>
    /// <param name="ctx">The context to copy, if non-null</param>
    public ContextData(ContextData ctx) {
        if (ctx.map != null && ctx.map.Count > 0)
            this.map = new Dictionary<string, object>(ctx.map);
    }

    public ContextData(IContextData context) => this.Merge(context);

    public ContextData Set<T>(DataKey<T> key, T? value) => this.SetRaw(key.Id, value);

    public ContextData Set(DataKey<bool> key, bool? value) => this.SetRaw(key.Id, value.BoxNullable());

    public ContextData SetRaw(string key, object? value) {
        if (value == null) {
            this.map?.Remove(key);
        }
        else {
            (this.map ??= new Dictionary<string, object>())[key] = value;
        }

        return this;
    }

    /// <summary>
    /// Tries to replace an existing value with the given value or tries to remove the given key.
    /// Returns true when value is null and the key is in this context data, or when value is non-null
    /// and was not in this context data. Returns false when value is null and the key did not exist or
    /// the value is non-null and existed in this context data
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool TryReplace<T>(DataKey<T> key, T value) => this.TryReplaceRaw(key.Id, value);

    public bool TryReplaceRaw(string key, object? value) {
        if (value == null) {
            return this.map != null && this.map.Remove(key);
        }
        else if (this.map == null || this.map.TryGetValue(key, out object? oldVal) && value.Equals(oldVal)) {
            return false;
        }
        else {
            this.map[key] = value;
            return true;
        }
    }

    public bool TryGetContext(string key, out object value) {
        if (this.map != null && this.map.TryGetValue(key, out value!))
            return true;
        value = null!;
        return false;
    }

    public bool ContainsKey(string key) => this.map != null && this.map.ContainsKey(key);

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

    public ContextData? ToNullIfEmpty() => this.Count > 0 ? this : null;

    public ContextData Merge(IContextData ctx) {
        if (ctx is ContextData cd && cd.map != null) {
            using Dictionary<string, object>.Enumerator enumerator = cd.map.GetEnumerator();
            if (!enumerator.MoveNext())
                return this;

            Dictionary<string, object> myMap = this.map ??= new Dictionary<string, object>();
            do {
                KeyValuePair<string, object> entry = enumerator.Current;
                myMap[entry.Key] = entry.Value;
            } while (enumerator.MoveNext());
        }

        return this;
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
        return new ContextData(dataA).Merge(dataB);
    }
}