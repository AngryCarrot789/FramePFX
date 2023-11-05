using System;
using System.Collections.Generic;

namespace FramePFX.Utils.Collections {
    /// <summary>
    /// A dictionary that maps a type to a value, which can be inherited from an entry of any base type unless explicitly set
    /// </summary>
    public class InheritanceDictionary<T> {
        private readonly Dictionary<Type, TypeEntry> items;
        private TypeEntry rootEntry;
        private int version;
        private int totalEntries;

        public bool IsEmpty => this.totalEntries <= 1;

        public T this[Type key] {
            get => this.GetEffectiveValue(key);
            set => this.SetValue(key, value);
        }

        public InheritanceDictionary() : this(int.MinValue) {
        }

        public InheritanceDictionary(int initialMapCapacity) {
            this.items = initialMapCapacity == int.MinValue ? new Dictionary<Type, TypeEntry>() : new Dictionary<Type, TypeEntry>(initialMapCapacity);
            this.items[typeof(object)] = this.rootEntry = new TypeEntry(this, typeof(object));
            this.totalEntries = 1;
        }

        private bool TryGetEntrySlow(Type key, out TypeEntry entry) {
            if (this.items.TryGetValue(key, out entry)) {
                return true;
            }

            for (Type type = key.BaseType; type != null; type = type.BaseType) {
                if (this.items.TryGetValue(type, out entry)) {
                    return true;
                }
            }

            return false;
        }

        private TypeEntry GetOrCreateEntryInternal(Type key) {
            if (this.items.TryGetValue(key, out TypeEntry entry)) {
                return entry;
            }

            if (key.IsInterface) {
                throw new Exception("Interfaces are not allowed in an InheritanceDictionary");
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
                this.items[typeof(object)] = this.rootEntry;
            return this.BakeTypes(this.rootEntry, types);
        }

        /// <summary>
        /// Gets the fully inherited value for the given type. If there is no value available, then the default value of T is returned
        /// </summary>
        /// <param name="key">The key to get the local (or inherited) value of</param>
        /// <returns>The effective value, or default</returns>
        public T GetEffectiveValue(Type key) {
            return this.GetOrCreateEntryInternal(key).inheritedItem;
        }

        /// <summary>
        /// Gets the fully inherited value for the given type. If there is no value available, then the default value of T is returned
        /// </summary>
        /// <param name="key">The key to get the local (or inherited) value of</param>
        /// <returns>The effective value, or default</returns>
        public T GetLocalValue(Type key) {
            return this.GetOrCreateEntryInternal(key).item;
        }

        /// <summary>
        /// Tries to get the effective value for the given type. If there are no inherited values available,
        /// then false is returned, otherwise true is returned and value is valid
        /// </summary>
        /// <param name="key">The key to get the local (or inherited) value of</param>
        /// <param name="value">The value found</param>
        /// <returns>True if a value was available, false if not</returns>
        public bool TryGetEffectiveValue(Type key, out T value) {
            TypeEntry entry = this.GetOrCreateEntryInternal(key);
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

        /// <summary>
        /// Gets or creates an entry for the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ITypeEntry<T> GetOrCreateEntry(Type key) => this.GetOrCreateEntryInternal(key);

        /// <summary>
        /// Slowly gets an entry for the given key, by manually iterating the type hierarchy if the given
        /// key does not already exist. Returns null if an entry could not be found. This method does not
        /// mutate this dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ITypeEntry<T> GetEntrySlowlyOrNull(Type key) => this.TryGetEntrySlow(key, out TypeEntry entry) ? entry : null;

        /// <summary>
        /// Explicitly sets (or replaces) the local value of the given key, and updates any
        /// derived types' inherited values if they are not already explicitly set
        /// </summary>
        /// <param name="key">The type to set the value for</param>
        /// <param name="value">The value to set</param>
        /// <returns>The previous local or inherited value</returns>
        public T SetValue(Type key, T value) {
            return this.GetOrCreateEntryInternal(key).SetItemAndUpdateInherited(value);
        }

        /// <summary>
        /// Explicitly sets (or replaces) the local value of the given entry, and updates any
        /// derived types' inherited values if they are not already explicitly set
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public T SetValue(ITypeEntry<T> entry, T value) {
            if (!(entry is TypeEntry te) || te.Owner != this)
                throw new ArgumentException("Invalid entry");
            return te.SetItemAndUpdateInherited(value);
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
            this.items[typeof(object)] = this.rootEntry = new TypeEntry(this, typeof(object));
            this.totalEntries = 1;
            this.version++;
        }

        /// <summary>
        /// Clears the local value for the given key, possibly updating the inherited value for any derived types
        /// </summary>
        /// <param name="key">The key to clear/remove</param>
        public void Clear(Type key) {
            if (this.items.TryGetValue(key, out TypeEntry entry)) {
                entry.OnClear();
                this.version++;
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
        public LocalValueEntryEnumerator GetLocalValueEnumerator(Type key, bool canMutate = true) {
            return new LocalValueEntryEnumerator(this, key, canMutate);
        }

        /// <summary>
        /// Gets a new enumerator struct whose GetEnumerator function returns a <see cref="LocalValueEntryEnumerator"/>
        /// </summary>
        /// <param name="key">The starting key</param>
        /// <returns>The enumerable struct</returns>
        public LocalValueEntryEnumerable GetLocalValueEnumerable(Type key, bool canMutate = true) {
            return new LocalValueEntryEnumerable(this, key, canMutate);
        }

        /// <summary>
        /// A struct that allows enumerating the local values of a type hierarchy, starting at
        /// a specific top-level type and navigating through the base types in an efficient manner
        /// </summary>
        public struct LocalValueEntryEnumerator { // : IEnumerator<ITypeEntry<T>>
            /// <summary>
            /// Gets the current entry
            /// </summary>
            public ITypeEntry<T> Current => this.currentEntry;

            public T CurrentValue => this.currentEntry.item;

            // object IEnumerator.Current => this.Current;

            private readonly InheritanceDictionary<T> dictionary;
            private readonly Type startingKey;
            private readonly bool canMutate;
            private TypeEntry currentEntry;
            private int state;
            private int version;
            private int depth;

            public int EnumerationDepth => this.depth;

            public LocalValueEntryEnumerator(InheritanceDictionary<T> dictionary, Type key, bool canMutate) {
                this.canMutate = canMutate;
                this.dictionary = dictionary;
                this.startingKey = key;
                this.currentEntry = null;
                this.depth = this.version = 0;

                // 0 = invalid,
                // 1 = ready,
                // 2 = enumerating,
                // 3 = finished
                // 4 = finished, because the first MoveNext() failed
                this.state = 1;
            }

            /// <summary>
            /// Moves the numerator to the next base entry with a local value available. If no base type could be
            /// found, then false is returned, otherwise true is returned and <see cref="Current"/> contains an
            /// entry with a local value
            /// </summary>
            /// <returns>See above</returns>
            /// <exception cref="InvalidOperationException">Concurrent modification (the dictionary was modified during enumeration)</exception>
            /// <exception cref="InvalidProgramException">This really should not occur but this is thrown when the dictionary is broken or gets corrupted</exception>
            public bool MoveNext() {
                switch (this.state) {
                    case 0: return false;
                    case 1: {
                        // slower at startup until all types are baked, making for fast reads
                        TypeEntry entry;
                        if (this.canMutate) {
                            entry = this.dictionary.GetOrCreateEntryInternal(this.startingKey);
                        }
                        else if (!this.dictionary.TryGetEntrySlow(this.startingKey, out entry)) {
                            this.state = 4;
                            return false;
                        }

                        if (!entry.HasLocalValue) {
                            entry = entry.nearestBaseTypeToExplicitValue;
                            if (entry == null) {
                                this.state = 4;
                                return false;
                            }
                            else if (!entry.HasLocalValue) {
                                throw new InvalidProgramException("Fatal error: nearest base type did not have an explicitly set value");
                            }
                        }

                        this.version = this.dictionary.version;
                        this.currentEntry = entry;
                        this.state = 2;
                        this.depth++;
                        return true;
                    }
                    case 2: {
                        if (this.dictionary.version != this.version) {
                            throw new InvalidOperationException("Concurrent modification of the original dictionary while enumerating");
                        }

                        if ((this.currentEntry = this.currentEntry.nearestBaseTypeToExplicitValue) != null) {
                            if (this.currentEntry.HasLocalValue) {
                                this.depth++;
                                return true;
                            }

                            // lol invalid program
                            throw new InvalidProgramException("Fatal error: nearest base type did not have an explicitly set value");
                        }

                        this.state = 3;
                        return false;
                    }
                    default: return false;
                }
            }

            /// <summary>
            /// Reset this enumerator to the original state
            /// </summary>
            public void Reset() {
                this.state = 1;
                this.currentEntry = null;
            }

            // void IDisposable.Dispose() => this.Reset();
        }

        /// <summary>
        /// A struct that has a <see cref="GetEnumerator"/> function, which returns a <see cref="LocalValueEntryEnumerator"/>
        /// </summary>
        public readonly struct LocalValueEntryEnumerable { //  : IEnumerable<ITypeEntry<T>>
            private readonly InheritanceDictionary<T> dictionary;
            private readonly Type startingKey;
            private readonly bool canMutate;

            public LocalValueEntryEnumerable(InheritanceDictionary<T> dictionary, Type key, bool canMutate) {
                this.dictionary = dictionary;
                this.startingKey = key;
                this.canMutate = canMutate;
            }

            public LocalValueEntryEnumerator GetEnumerator() {
                return new LocalValueEntryEnumerator(this.dictionary, this.startingKey, this.canMutate);
            }

            // IEnumerator<ITypeEntry<T>> IEnumerable<ITypeEntry<T>>.GetEnumerator() => this.GetEnumerator();
            // IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }

        // Baked a hierarchy of types starting from 1 above foundType to the given key (base to derived)
        // foundEntry is a non-null reference to the lowest entry that exists in the map
        private TypeEntry BakeTypes(TypeEntry foundEntry, List<Type> types) {
            // types is an ordered list of the topType all the way down to before the foundEntry's type (exclusive)
            // the key being inserted will be types[0]
            TypeEntry entry, baseType = foundEntry;
            int i = types.Count - 1, c = this.totalEntries;
            do {
                entry = new TypeEntry(this, types[i]);
                this.items[types[i]] = entry;
                entry.OnExtendType(baseType);
                baseType = entry;
                c++;
            } while (--i >= 0);
            this.totalEntries = c;
            this.version++;
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
            public readonly InheritanceDictionary<T> Owner;

            public TypeEntry(InheritanceDictionary<T> owner, Type type) {
                this.Owner = owner;
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
                for (TypeEntry super = superType; super != null; super = super.baseType) {
                    if (super.HasLocalValue) {
                        this.nearestBaseTypeToExplicitValue = super;
                    }
                    else if (super.nearestBaseTypeToExplicitValue != null) {
                        this.nearestBaseTypeToExplicitValue = super.nearestBaseTypeToExplicitValue;
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