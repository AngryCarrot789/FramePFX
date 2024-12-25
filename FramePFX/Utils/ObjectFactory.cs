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

using System.Diagnostics.CodeAnalysis;

namespace FramePFX.Utils;

/// <summary>
/// A class that helps with object creation based on a unique string identifier
/// </summary>
public class ObjectFactory {
    private readonly Dictionary<string, Type> idToType;
    private readonly Dictionary<Type, string> typeToId;

    public ObjectFactory() {
        this.idToType = new Dictionary<string, Type>();
        this.typeToId = new Dictionary<Type, string>();
    }

    protected virtual bool IsTypeValid(Type type) {
        return true;
    }

    protected void RegisterType(string id, Type type) {
        ValidateId(id);
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (type.IsAbstract || type.IsInterface)
            throw new InvalidOperationException("The type is abstract or an interface and therefore cannot be used");
        if (!this.IsTypeValid(type))
            throw new ArgumentException($"Incompatible type: {type.Name}", nameof(type));
        if (this.idToType.TryGetValue(id, out Type? existingType))
            throw new InvalidOperationException($"ID '{id}' already registered with type '{existingType.Name}'");
        if (this.typeToId.TryGetValue(type, out string? existingId))
            throw new InvalidOperationException($"Type '{type.Name}' already registered with ID '{existingId}'");
        this.idToType[id] = type;
        this.typeToId[type] = id;
        this.OnRegistered(id, type);
    }

    protected bool UnregisterType(string id) {
        ValidateId(id);
        if (!this.idToType.Remove(id, out Type? type))
            return false;

        this.OnUnregistered(id, type);
        return true;
    }

    protected virtual void OnRegistered(string id, Type type) {
    }

    protected virtual void OnUnregistered(string id, Type type) {
    }

    public bool IsIdRegistered(string id) {
        ValidateId(id);
        return this.idToType.ContainsKey(id);
    }

    public bool IsTypeRegistered(Type type) {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        return this.typeToId.ContainsKey(type);
    }

    public bool TryGetType(string id, [NotNullWhen(true)] out Type? type) {
        ValidateId(id);
        return this.idToType.TryGetValue(id, out type);
    }

    public bool TryGetId(Type type, [NotNullWhen(true)] out string? id) {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        return this.typeToId.TryGetValue(type, out id);
    }

    public Type GetType(string id) {
        ValidateId(id);
        if (!this.idToType.TryGetValue(id, out Type? type))
            throw new Exception($"No entry registered with ID '{id}'");
        return type;
    }

    public string GetId(Type type) {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (!this.typeToId.TryGetValue(type, out string? id))
            throw new Exception($"No entry registered with type '{type.Name}'");
        if (string.IsNullOrWhiteSpace(id))
            throw new Exception("Invalid factory ID");
        return id;
    }

    private static void ValidateId(string id) {
        if (id == null)
            throw new ArgumentNullException(nameof(id));
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be an empty string or consist of only whitespaces", nameof(id));
    }
}