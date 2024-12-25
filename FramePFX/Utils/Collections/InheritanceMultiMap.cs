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

namespace FramePFX.Utils.Collections;

/// <summary>
/// A dictionary that maps a type to a collection of values, where the values are the accumulation of each base type's value
/// </summary>
[Obsolete("Mostly untested, not much use case at the moment, and this class also has a few things broken")]
public class InheritanceMultiMap<T> {
    private readonly Dictionary<Type, TypeEntry> items;
    private readonly int initialListCapacity;
    private readonly List<T> singletonList;
    private readonly TypeEntry rootEntry;
    public static readonly IReadOnlyList<T> EmptyList = new List<T>().AsReadOnly();

    public InheritanceMultiMap() : this(int.MinValue, int.MinValue) {
    }

    public InheritanceMultiMap(int initialMapCapacity, int initialListCapacity) {
        if (initialListCapacity != int.MinValue && initialListCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialListCapacity), "Initial list capacity must either be int.MinValue (as in, undefined) or a non-negative number");
        this.items = initialMapCapacity == int.MinValue ? new Dictionary<Type, TypeEntry>() : new Dictionary<Type, TypeEntry>(initialMapCapacity);
        this.initialListCapacity = initialListCapacity;
        this.singletonList = new List<T>() { default };
        this.rootEntry = new TypeEntry(this, typeof(object));
        this.InsertRootEntry();
    }

    private void InsertRootEntry() => this.items.Add(typeof(object), this.rootEntry);

    private TypeEntry GetOrCreateEntry(Type key) {
        if (this.items.TryGetValue(key, out TypeEntry? entry)) {
            return entry;
        }

        List<Type> types = new List<Type> { key };
        for (Type type = key.BaseType; type != null; type = type.BaseType) {
            if (this.items.TryGetValue(type, out entry)) {
                return this.BakeType(entry, types);
            }
            else {
                types.Add(type);
            }
        }

#if DEBUG
        throw new Exception("Fatal error: object BaseType not found in the internal map");
#endif
        // THIS SHOULD NOT BE POSSIBLE!!!!!!!!!!!!!!!!!!!!
        if (!this.items.TryGetValue(typeof(object), out entry))
            this.items[typeof(object)] = entry = new TypeEntry(this, typeof(object));
        return this.BakeType(entry, types);
    }

    /// <summary>
    /// Gets a list of inherited values for the given key. The values of each derived type
    /// are inherited by the values of each base type
    /// </summary>
    /// <param name="key">The top-level type to get the values of</param>
    /// <returns>A list containing the values of the given key's entry, and all of its base types' values</returns>
    public IReadOnlyList<T> GetValues(Type key) {
        return this.GetOrCreateEntry(key).inheritedItems;
    }

    /// <summary>
    /// Gets a list of items that are not inherited by any base types. If the key is not in
    /// this multimap, then <see cref="EmptyList"/> is returned
    /// </summary>
    /// <param name="key">The key to get the non-inherited values of</param>
    /// <returns>The list of values, or the EmptyList instance</returns>
    public IReadOnlyList<T> GetLocalValues(Type key) {
        return this.items.TryGetValue(key, out TypeEntry? entry) ? entry.items : EmptyList;
    }

    /// <summary>
    /// Adds a value to the given type's local value list, and it's own and all derived types' inherited values lists
    /// </summary>
    /// <param name="key">The key to add the value to</param>
    /// <param name="value">The value to add</param>
    public void Add(Type key, T value) {
        this.singletonList[0] = value;
        this.GetOrCreateEntry(key).AddItemsAndBakeHierarchy(this.singletonList);
        this.singletonList[0] = default;
    }

    /// <summary>
    /// Adds a collection of values to the given type's local value list, and it's own and all derived types' inherited values lists
    /// </summary>
    /// <param name="key">The key to add the values to</param>
    /// <param name="value">The values to add</param>
    public void AddRange(Type key, ICollection<T> value) {
        this.GetOrCreateEntry(key).AddItemsAndBakeHierarchy(value);
    }

    /// <summary>
    /// Creates a new list containing the given values, and then invokes <see cref="AddRange(Type,ICollection{T})"/>
    /// with that this, as the collection may be enumerated multiple times
    /// </summary>
    /// <param name="key">The key to add the values to</param>
    /// <param name="value">The values to add</param>
    public void AddRange(Type key, IEnumerable<T> value) => this.AddRange(key, value.ToList());

    /// <summary>
    /// Clears all values for all keys in this map
    /// </summary>
    public void Clear() {
        foreach (TypeEntry map in this.items.Values) {
            map.Dispose();
        }

        this.items.Clear();
        this.InsertRootEntry();
    }

    /// <summary>
    /// Clears all values for the given key and all derived types, if an entry exists for the given key
    /// </summary>
    /// <param name="key">The key</param>
    public void Clear(Type key) {
        if (this.items.TryGetValue(key, out TypeEntry? entry)) {
            entry.Clear();
        }
    }

    // Baked a hierarchy of types starting from 1 above foundType to the given key (base to derived)
    // foundEntry is a non-null reference to the lowest entry that exists in the map
    private TypeEntry BakeType(TypeEntry foundEntry, List<Type> types) {
        // types is an ordered list of the topType all the way down to before the foundEntry's type (exclusive)
        // the key being inserted will be types[0]
        TypeEntry entry;
        int i = types.Count - 1;
        do {
            entry = new TypeEntry(this, types[i]);
            this.items[types[i]] = entry;
            entry.OnExtendType(foundEntry);
        } while (--i >= 0);

        return entry;
    }

    private class TypeEntry {
        private readonly InheritanceMultiMap<T> map;
        public readonly Type type;
        public readonly List<T> items;
        public readonly List<T> inheritedItems;
        public readonly List<TypeEntry> derivedList;
        public TypeEntry baseType;
        public readonly bool IsRoot;

        public TypeEntry(InheritanceMultiMap<T> map, Type type) {
            this.IsRoot = type == typeof(object);
            this.map = map;
            this.type = type;
            this.derivedList = new List<TypeEntry>(1);
            if (map.initialListCapacity != int.MinValue) {
                this.items = new List<T>(map.initialListCapacity);
                this.inheritedItems = new List<T>(map.initialListCapacity);
            }
            else {
                this.items = new List<T>();
                this.inheritedItems = new List<T>();
            }
        }

        // we now derived from the base type
        public void OnExtendType(TypeEntry baseType) {
            if (baseType.derivedList.Contains(this))
                throw new Exception("Base entry is already derived by this entry");
            if (this.baseType != null)
                throw new Exception("This entry has already been derived once from another type: " + this.baseType.type);
            if (baseType.type != this.type.BaseType)
                throw new Exception("Fatal error: the current type's base type does not equal the given base entry's type");

            baseType.derivedList.Add(this);
            this.baseType = baseType;
            this.inheritedItems.AddRange(baseType.inheritedItems);
        }

        public void AddItemsAndBakeHierarchy(ICollection<T> collection) {
            this.items.AddRange(collection);
            this.AddToOurBakedListAndDerivedTypes(collection);
        }

        private void AddToOurBakedListAndDerivedTypes(ICollection<T> list) {
            this.inheritedItems.AddRange(list);
            for (int i = this.derivedList.Count - 1; i >= 0; i--) {
                this.derivedList[i].AddToOurBakedListAndDerivedTypes(list);
            }
        }

        // this just clears all derived types' inherited items, and then re-bakes them all.
        // This is most likely much faster than doing a remove-by-item for each derived types'
        // inherited items list as that would be `On2`, rather than `On` with a bit extra due to the recursion

        public void Clear() {
            this.items.Clear();
            this.RebakeSelfAndDerivedTypesInheritedItems();
        }

        private void RebakeSelfAndDerivedTypesInheritedItems() {
            this.inheritedItems.Clear();
            if (this.baseType != null) {
                this.inheritedItems.AddRange(this.baseType.inheritedItems);
            }

            foreach (TypeEntry derivedType in this.derivedList) {
                derivedType.RebakeSelfAndDerivedTypesInheritedItems();
            }
        }

        // remove all items to help the GC out a little bit due to the complex reference structure
        public void Dispose() {
            this.baseType = null;
            this.derivedList.Clear();
            this.items.Clear();
            this.inheritedItems.Clear();
            this.derivedList.TrimExcess();
            this.items.TrimExcess();
            this.inheritedItems.TrimExcess();
        }
    }
}