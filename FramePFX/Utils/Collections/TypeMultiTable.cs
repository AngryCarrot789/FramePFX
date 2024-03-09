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

using System;
using System.Collections.Generic;

namespace FramePFX.Utils.Collections
{
    /// <summary>
    /// Similar to a <see cref="InheritanceMultiMap{T}"/> but its a table, where you map to 2 types to a list of values
    /// </summary>
    public class TypeMultiTable<T>
    {
        private readonly Dictionary<Type, TypeEntry> items;
        private readonly int initialEntryListCapacity;
        private readonly TypeEntry rootEntry;

        public TypeMultiTable() : this(int.MinValue, int.MinValue)
        {
        }

        public TypeMultiTable(int initialMapCapacity, int initialEntryListCapacity)
        {
            if (initialEntryListCapacity != int.MinValue && initialEntryListCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialEntryListCapacity), "Initial list capacity must either be int.MinValue (as in, undefined) or a non-negative number");
            this.items = initialMapCapacity == int.MinValue ? new Dictionary<Type, TypeEntry>() : new Dictionary<Type, TypeEntry>(initialMapCapacity);
            this.initialEntryListCapacity = initialEntryListCapacity;
            this.rootEntry = new TypeEntry(this, typeof(object));
            this.InsertRootEntry();
        }

        private void InsertRootEntry() => this.items.Add(typeof(object), this.rootEntry);

        private TypeEntry GetOrCreateEntry(Type key)
        {
            if (this.items.TryGetValue(key, out TypeEntry entry))
            {
                return entry;
            }

            List<Type> types = new List<Type> {key};
            for (Type type = key.BaseType; type != null; type = type.BaseType)
            {
                if (this.items.TryGetValue(type, out entry))
                {
                    return this.BakeType(entry, types);
                }
                else
                {
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
        /// Gets the final inherited value for the given row and column
        /// </summary>
        /// <param name="row">The primary key</param>
        /// <param name="column">The secondary key</param>
        /// <param name="value">The output value, or default, if no such entry exists of the row or column</param>
        /// <returns>True if an entry exists for the row and column, otherwise false</returns>
        public bool TryGetValue(Type row, Type column, out T value)
        {
            return this.GetOrCreateEntry(column).GetValue(row, out value);
        }

        /// <summary>
        /// Tries to get the local non-inherited value for the given row and column
        /// </summary>
        /// <param name="row">The primary key</param>
        /// <param name="column">The secondary key</param>
        /// <param name="value">The output value, or default, if no such entry exists of the row or column</param>
        /// <returns>True if an entry exists for the row and column, otherwise false</returns>
        public bool TryGetLocalValue(Type row, Type column, out T value)
        {
            return this.GetOrCreateEntry(column).GetLocalValue(row, out value);
        }

        /// <summary>
        /// Adds or replaces a value to the given type's local value dictionary, and it's own and all derived types' inherited dictionaries
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        public void Add(Type row, Type column, T value)
        {
            this.GetOrCreateEntry(row).AddItemAndSetupInheritedHierarchy(column, value);
        }

        /// <summary>
        /// Clears all values for all keys in this map
        /// </summary>
        public void Clear()
        {
            foreach (TypeEntry map in this.items.Values)
            {
                map.Dispose();
            }

            this.items.Clear();
            this.InsertRootEntry();
        }

        /// <summary>
        /// Clears all values for the given key and all derived types, if an entry exists for the given key
        /// </summary>
        /// <param name="key">The key</param>
        public void Clear(Type key)
        {
            if (this.items.TryGetValue(key, out TypeEntry entry))
            {
                entry.Clear();
            }
        }

        /// <summary>
        /// Gets an enumerator that enumerates the inherited
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public IEnumerable<T> GetEnumerator(Type row, Type column)
        {
            return null;
        }

        // Baked a hierarchy of types starting from 1 above foundType to the given key (base to derived)
        // foundEntry is a non-null reference to the lowest entry that exists in the map
        private TypeEntry BakeType(TypeEntry foundEntry, List<Type> types)
        {
            // types is an ordered list of the topType all the way down to before the foundEntry's type (exclusive)
            // the key being inserted will be types[0]
            TypeEntry entry, baseType = foundEntry;
            int i = types.Count - 1;
            do
            {
                entry = new TypeEntry(this, types[i]);
                this.items[types[i]] = entry;
                entry.OnExtendType(baseType);
                baseType = entry;
            } while (--i >= 0);

            return entry;
        }

        private class TypeEntry
        {
            public readonly Type type;
            private readonly Dictionary<Type, T> items;
            private readonly Dictionary<Type, T> inheritedItems;
            public readonly List<TypeEntry> derivedList;
            public readonly List<TypeEntry> baseTypeList;
            public readonly bool IsRoot;

            public int ItemsCount => this.items.Count;
            public int InheritedItemsCount => this.inheritedItems.Count;

            public TypeEntry(TypeMultiTable<T> map, Type type)
            {
                this.IsRoot = type == typeof(object);
                this.type = type;
                this.derivedList = new List<TypeEntry>(1);
                this.baseTypeList = new List<TypeEntry>(1);

                if (map.initialEntryListCapacity != int.MinValue)
                {
                    this.items = new Dictionary<Type, T>(map.initialEntryListCapacity);
                    this.inheritedItems = new Dictionary<Type, T>(map.initialEntryListCapacity);
                }
                else
                {
                    this.items = new Dictionary<Type, T>();
                    this.inheritedItems = new Dictionary<Type, T>();
                }
            }

            // we now derived from the base type
            public void OnExtendType(TypeEntry baseType)
            {
                if (baseType.derivedList.Contains(this))
                    throw new Exception("Base entry is already derived by this entry");
                if (this.baseTypeList.Contains(baseType))
                    throw new Exception("This entry is already derived from the base entry");
                if (baseType.type != this.type.BaseType)
                    throw new Exception("Fatal error: the current type's base type does not equal the given base entry's type");

                baseType.derivedList.Add(this);
                this.baseTypeList.Add(baseType);

                foreach (KeyValuePair<Type, T> entry in baseType.inheritedItems)
                {
                    this.inheritedItems[entry.Key] = entry.Value;
                }
            }

            public void AddItemAndSetupInheritedHierarchy(Type key, T value)
            {
                this.items[key] = value;
                this.SetInheritedValue(key, ref value);
            }

            private void SetInheritedValue(Type key, ref T value)
            {
                this.inheritedItems[key] = value;
                for (int i = this.derivedList.Count - 1; i >= 0; i--)
                {
                    TypeEntry derived = this.derivedList[i];
                    if (!derived.items.ContainsKey(key))
                    {
                        derived.SetInheritedValue(key, ref value);
                    }
                }
            }

            // this just clears all derived types' inherited items, and then re-bakes them all.
            // This is most likely much faster than doing a remove-by-item for each derived types'
            // inherited items list as that would be `On2`, rather than `On` with a bit extra due to the recursion

            public void Clear()
            {
                this.items.Clear();
                this.RebakeSelfAndDerivedTypesInheritedItems();
            }

            private void RebakeSelfAndDerivedTypesInheritedItems()
            {
                this.inheritedItems.Clear();
                foreach (TypeEntry baseType in this.baseTypeList)
                {
                    foreach (KeyValuePair<Type, T> entry in baseType.inheritedItems)
                    {
                        this.inheritedItems[entry.Key] = entry.Value;
                    }
                }

                foreach (TypeEntry derivedType in this.derivedList)
                {
                    derivedType.RebakeSelfAndDerivedTypesInheritedItems();
                }
            }

            // remove all items to help the GC out a little bit due to the complex reference structure
            public void Dispose()
            {
                this.derivedList.Clear();
                this.baseTypeList.Clear();
                this.items.Clear();
                this.inheritedItems.Clear();

                this.derivedList.TrimExcess();
                this.baseTypeList.TrimExcess();
            }

            public bool GetValue(Type key, out T value) => this.inheritedItems.TryGetValue(key, out value);
            public bool GetLocalValue(Type key, out T value) => this.items.TryGetValue(key, out value);
        }
    }
}