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

using System.Diagnostics.CodeAnalysis;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Interactivity.Contexts;

public abstract class DataKey {
    private static readonly Dictionary<string, DataKey> Registry;

    /// <summary>
    /// A unique identifier for this data key
    /// </summary>
    public string Id { get; }

    protected DataKey(string id) {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be null, empty or consist of only whitespaces");
        this.Id = id;
    }

    static DataKey() {
        Registry = new Dictionary<string, DataKey>();
    }

    public static DataKey? GetKeyById(string id) {
        lock (Registry)
            return Registry.GetValueOrDefault(id);
    }

    protected static void RegisterInternal(string id, DataKey key) {
        Validate.NotNullOrWhiteSpaces(id);
        Validate.NotNull(key);
        lock (Registry) {
            if (!Registry.TryAdd(id, key))
                throw new InvalidOperationException("ID already in use: " + id);
        }
    }

    public static bool operator ==(DataKey a, DataKey b) {
        return ReferenceEquals(a, b) || !ReferenceEquals(a, null) && !ReferenceEquals(b, null) && a.Equals(b);
    }

    public static bool operator !=(DataKey a, DataKey b) {
        return !ReferenceEquals(a, b) && (ReferenceEquals(a, null) || ReferenceEquals(b, null) || !a.Equals(b));
    }

    protected bool Equals(DataKey other) {
        return this.Id == other.Id;
    }

    public override bool Equals(object? obj) {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        return obj is DataKey key && this.Equals(key);
    }

    public override int GetHashCode() => this.Id.GetHashCode();

    public override string ToString() => $"DataKey(\"{this.Id}\")";
}

public sealed class DataKey<T> : DataKey {
    private DataKey(string id) : base(id) { }

    public static DataKey<T> Create(string id) {
        DataKey<T> key = new DataKey<T>(id);
        RegisterInternal(id, key);
        return key;
    }

    public bool IsPresent(IContextData contextData) => contextData.ContainsKey(this.Id);

    public Executability GetExecutabilityForPresence(IContextData contextData) => this.IsPresent(contextData) ? Executability.Valid : Executability.Invalid;

    public bool TryGetContext(IContextData context, [NotNullWhen(true)] out T? value) {
        Validate.NotNull(context);
        if (context.TryGetContext(this.Id, out object? obj)) {
            value = obj is T t ? t : throw new Exception($"Context contained an invalid value for this key: type mismatch ({typeof(T)} != {obj.GetType()})");
            return true;
        }
        else {
            value = default!;
            return false;
        }
    }

    public T? GetContext(IContextData context, T? def = default) {
        return this.TryGetContext(context, out T? value) ? value : def;
    }
}