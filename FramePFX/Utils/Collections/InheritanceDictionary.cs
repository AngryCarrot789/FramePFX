using System;
using System.Collections.Generic;

namespace FramePFX.Utils.Collections {
    /// <summary>
    /// A dictionary that maps a type to a value, which can be inherited from an entry of any base type unless explicitly set
    /// </summary>
    public class InheritanceDictionary<T> {
        private readonly Dictionary<Type, TypeEntry> items;
        private readonly TypeEntry rootEntry;

        public T this[Type key] {
            get => this.GetValue(key);
            set => this.SetValue(key, value);
        }

        public InheritanceDictionary() : this(int.MinValue) {
        }

        public InheritanceDictionary(int initialMapCapacity) {
            this.items = initialMapCapacity == int.MinValue ? new Dictionary<Type, TypeEntry>() : new Dictionary<Type, TypeEntry>(initialMapCapacity);
            this.rootEntry = new TypeEntry(typeof(object));
            this.InsertRootEntry();
        }

        private void InsertRootEntry() => this.items.Add(typeof(object), this.rootEntry);

        private TypeEntry FindNearestBaseTypeEntryOrSelf(Type type) {
            for (Type t = type; t != null; t = t.BaseType) {
                if (this.items.TryGetValue(t, out TypeEntry entry)) {
                    return entry;
                }
            }

            return null;
        }

        private TypeEntry GetOrCreateEntry(Type key) {
            if (this.items.TryGetValue(key, out TypeEntry entry)) {
                return entry;
            }

            List<Type> types = new List<Type> {key};
            for (Type type = key.BaseType; type != null; type = type.BaseType) {
                if (this.items.TryGetValue(type, out entry)) {
                    return this.BakeTypes(entry, types);
                }
                else {
                    types.Add(type);
                }
            }

#if DEBUG
            throw new Exception("Fatal error: object BaseType not found in the internal map");
#endif
            // THIS SHOULD NOT BE POSSIBLE!!!!!!!!!!!!!!!!!!!!
            if (!this.items.ContainsKey(typeof(object)))
                this.InsertRootEntry();
            return this.BakeTypes(this.rootEntry, types);
        }

        /// <summary>
        /// Gets the fully inherited value for the given type. If there is no value available, then the default value of T is returned
        /// </summary>
        /// <param name="key">The key to get the local (or inherited) value of</param>
        /// <returns>The effective value, or default</returns>
        public T GetValue(Type key) {
            return this.GetOrCreateEntry(key).inheritedItem;
        }

        /// <summary>
        /// Gets the fully inherited value for the given type. If there is no value available, then the default value of T is returned
        /// </summary>
        /// <param name="key">The key to get the local (or inherited) value of</param>
        /// <returns>The effective value, or default</returns>
        public T GetLocalValue(Type key) {
            return this.GetOrCreateEntry(key).item;
        }

        /// <summary>
        /// Tries to get the effective value for the given type. If there are no inherited values available,
        /// then false is returned, otherwise true is returned and value is valid
        /// </summary>
        /// <param name="key">The key to get the local (or inherited) value of</param>
        /// <param name="value">The value found</param>
        /// <returns>True if a value was available, false if not</returns>
        public bool TryGetValue(Type key, out T value) {
            TypeEntry entry = this.GetOrCreateEntry(key);
            value = entry.inheritedItem;
            return entry.HasInheritedValue;
        }

        /// <summary>
        /// Tries to get the local value for the given type
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetLocalValue(Type key, out T value) {
            if (this.items.TryGetValue(key, out TypeEntry entry) && entry.HasLocalValue) {
                value = entry.item;
                return true;
            }

            value = default;
            return false;
        }

        public ITypeEntry<T> GetEntry(Type key) => this.GetOrCreateEntry(key);

        /// <summary>
        /// Either returns the entry for the given type, or finds the first entry available by navigating the given type's BaseTypes
        /// </summary>
        /// <param name="type">The starting type</param>
        /// <returns>The first available entry as the given type or as one of its base types</returns>
        public ITypeEntry<T> FindNearestBaseType(Type type) {
            return this.FindNearestBaseTypeEntryOrSelf(type) ?? this.GetOrCreateEntry(type);
        }

        /// <summary>
        /// Explicitly sets (or replaces) the local value of the given key, and updates any
        /// derived types' inherited values if they are not already explicitly set
        /// </summary>
        /// <param name="key">The type to set the value for</param>
        /// <param name="value">The value to set</param>
        /// <returns>The previous local or inherited value</returns>
        public T SetValue(Type key, T value) {
            return this.GetOrCreateEntry(key).SetItemAndUpdateInherited(value);
        }

        /// <summary>
        /// Returns true if the entry for the given type has a local value set, otherwise, returns false (meaning it may or may not have an inherited value)
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool HasLocalValue(Type type) {
            return this.items.TryGetValue(type, out TypeEntry entry) && entry.HasLocalValue;
        }

        /// <summary>
        /// Clears all values for all keys in this map
        /// </summary>
        public void Clear() {
            foreach (TypeEntry map in this.items.Values)
                map.Dispose();
            this.items.Clear();
            this.InsertRootEntry();
        }

        /// <summary>
        /// Clears the local value for the given key, possibly updating the inherited value for any derived types
        /// </summary>
        /// <param name="key">The key to clear/remove</param>
        public void Clear(Type key) {
            if (this.items.TryGetValue(key, out TypeEntry entry)) {
                entry.OnClear();
            }
        }

        /// <summary>
        /// Gets a new enumerator that allows enumerating the entries that have local values set
        /// </summary>
        /// <param name="key">
        /// The highest type in a type hierarchy which is where enumeration starts
        /// (includes the value of the key, if it has a value associated with it)
        /// </param>
        /// <returns>An enumerable instance</returns>
        public IEnumerable<ITypeEntry<T>> GetLocalValueEnumerator(Type key) {
            TypeEntry entry = this.FindNearestBaseTypeEntryOrSelf(key);
            if (entry == null) {
                yield break;
            }

            for (TypeEntry type = entry; type != null; type = type.nearestBaseTypeToExplicitValue) {
                if (type.HasLocalValue) {
                    yield return type;
                }
                else {
                    throw new Exception("Fatal error: nearest base type did not have an explicitly set value");
                }
            }
        }

        // Baked a hierarchy of types starting from 1 above foundType to the given key (base to derived)
        // foundEntry is a non-null reference to the lowest entry that exists in the map
        private TypeEntry BakeTypes(TypeEntry foundEntry, List<Type> types) {
            // types is an ordered list of the topType all the way down to before the foundEntry's type (exclusive)
            // the key being inserted will be types[0]
            TypeEntry entry, baseType = foundEntry;
            int i = types.Count - 1;
            do {
                entry = new TypeEntry(types[i]);
                this.items[types[i]] = entry;
                entry.OnExtendType(baseType);
                baseType = entry;
            } while (--i >= 0);

            return entry;
        }

        private class TypeEntry : ITypeEntry<T> {
            public readonly Type type;
            public T item;
            public T inheritedItem;
            public bool HasLocalValue, HasInheritedValue; // is item explicitly set
            public readonly List<TypeEntry> derivedList;
            public TypeEntry baseType;
            public TypeEntry nearestBaseTypeToExplicitValue;

            Type ITypeEntry<T>.Type => this.type;
            bool ITypeEntry<T>.HasLocalValue => this.HasLocalValue;
            bool ITypeEntry<T>.HasInheritedValue => this.HasInheritedValue;
            T ITypeEntry<T>.LocalValue => this.HasLocalValue ? this.item : throw new InvalidOperationException("No local value set");
            T ITypeEntry<T>.EffectiveValue => this.HasInheritedValue ? this.inheritedItem : throw new InvalidOperationException("No inherited value set");
            IReadOnlyList<ITypeEntry<T>> ITypeEntry<T>.DerivedTypes => this.derivedList;
            ITypeEntry<T> ITypeEntry<T>.BaseType => this.baseType;
            ITypeEntry<T> ITypeEntry<T>.NearestBaseTypeWithLocalValue => this.nearestBaseTypeToExplicitValue;

            public TypeEntry(Type type) {
                this.type = type;
                this.derivedList = new List<TypeEntry>(1);
            }

            public override string ToString() {
                return $"TypeEntry({this.type.Name} -> {this.baseType?.type.Name} (nearest local value = {this.nearestBaseTypeToExplicitValue?.type.Name}))";
            }

            // we now derived from the base type
            public void OnExtendType(TypeEntry superType) {
                if (superType.derivedList.Contains(this))
                    throw new Exception("Base entry is already derived by this entry");
                if (this.baseType != null)
                    throw new Exception("This entry has already been setup/derived once from another type: " + this.baseType.type);

                superType.derivedList.Add(this);
                this.baseType = superType;
                if (superType.HasInheritedValue)
                    this.inheritedItem = superType.inheritedItem;
                for (TypeEntry bt = superType; bt != null; bt = bt.baseType) {
                    if (bt.HasLocalValue) {
                        this.nearestBaseTypeToExplicitValue = bt;
                    }
                    else if (bt.nearestBaseTypeToExplicitValue != null) {
                        this.nearestBaseTypeToExplicitValue = bt.nearestBaseTypeToExplicitValue;
                    }
                    else {
                        continue;
                    }

                    break;
                }
            }

            public T SetItemAndUpdateInherited(T value) {
                T prev = this.HasInheritedValue ? this.inheritedItem : default;
                this.item = value;
                this.HasLocalValue = true;
                this.SetInheritedValue(this, ref value);
                return prev;
            }

            private void SetInheritedValue(TypeEntry setItemOrigin, ref T value) {
                this.HasInheritedValue = true;
                this.inheritedItem = value;
                for (int i = this.derivedList.Count - 1; i >= 0; i--) {
                    TypeEntry derived = this.derivedList[i];
                    derived.nearestBaseTypeToExplicitValue = setItemOrigin;
                    if (!derived.HasLocalValue) {
                        derived.SetInheritedValue(setItemOrigin, ref value);
                    }
                }
            }

            // this just clears all derived types' inherited items, and then re-bakes them all.
            // This is most likely much faster than doing a remove-by-item for each derived types'
            // inherited items list as that would be `On2`, rather than `On` with a bit extra due to the recursion

            public void OnClear() {
                this.HasLocalValue = false;
                this.item = default;
                this.ClearInheritedAndRecalculateHierarchy();
            }

            private void ClearInheritedAndRecalculateHierarchy() {
                if (this.baseType != null) {
                    this.nearestBaseTypeToExplicitValue = this.baseType.nearestBaseTypeToExplicitValue;
                    if (this.baseType.HasInheritedValue) {
                        this.inheritedItem = this.baseType.inheritedItem;
                        this.HasInheritedValue = true;
                    }
                    else {
                        this.inheritedItem = default;
                        this.HasInheritedValue = false;
                    }
                }
                else {
                    this.nearestBaseTypeToExplicitValue = null;
                    this.inheritedItem = default;
                    this.HasInheritedValue = false;
                }

                foreach (TypeEntry derivedType in this.derivedList) {
                    derivedType.nearestBaseTypeToExplicitValue = this.nearestBaseTypeToExplicitValue;
                    if (!derivedType.HasLocalValue) {
                        derivedType.ClearInheritedAndRecalculateHierarchy();
                    }
                }
            }

            // remove all items to help the GC out a little bit due to the complex reference structure
            public void Dispose() {
                this.derivedList.Clear();
                this.baseType = null;
                this.item = default;
                this.inheritedItem = default;
                this.derivedList.TrimExcess();
            }
        }
    }
}