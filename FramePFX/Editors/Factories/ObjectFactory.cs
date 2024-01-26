using System;
using System.Collections.Generic;

namespace FramePFX.Editors.Factories {
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
            if (!this.IsTypeValid(type))
                throw new ArgumentException($"Incompatible type: {type.Name}", nameof(type));
            if (this.idToType.TryGetValue(id, out Type existingType))
                throw new InvalidOperationException($"ID '{id}' already registered with type '{existingType.Name}'");
            if (this.typeToId.TryGetValue(type, out string existingId))
                throw new InvalidOperationException($"Type '{type.Name}' already registered with ID '{existingId}'");
            this.idToType[id] = type;
            this.typeToId[type] = id;
            this.OnRegistered(id, type);
        }

        protected bool UnregisterType(string id) {
            ValidateId(id);
            if (!this.idToType.TryGetValue(id, out Type type))
                return false;

            this.idToType.Remove(id);
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

        public bool TryGetType(string id, out Type type) {
            ValidateId(id);
            return this.idToType.TryGetValue(id, out type);
        }

        public bool TryGetId(Type type, out string id) {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            return this.typeToId.TryGetValue(type, out id);
        }

        public Type GetType(string id) {
            ValidateId(id);
            if (!this.idToType.TryGetValue(id, out Type type))
                throw new Exception($"No entry registered with ID '{id}'");
            return type;
        }

        public string GetId(Type type) {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (!this.typeToId.TryGetValue(type, out string id))
                throw new Exception($"No entry registered with type '{type.Name}'");
            return id;
        }

        private static void ValidateId(string id) {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be an empty string or consist of only whitespaces", nameof(id));
        }
    }
}