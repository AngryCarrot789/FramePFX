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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace FramePFX.Utils.Collections {
    // UNUSED ATM... this is an almost-copy of SortedList which I hope to optimise for my own use cases
    public class DictionaryList<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue> where TKey : class {
        private const int DefaultCapacity = 4;
        private const int MaxArrayLength = 0X7FEFFFFF;
        private TKey[] keys;
        private TValue[] values;
        private int size;
        private int version;
        private readonly IComparer<TKey> comparer;
        private KeyList keyList;
        private ValueList valueList;

        private static readonly TKey[] EmptyKeys = new TKey[0];
        private static readonly TValue[] EmptyValues = new TValue[0];
        private object _syncRoot;

        public int Count => this.size;

        public IComparer<TKey> Comparer => this.comparer;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => this.GetKeyListHelper();

        ICollection IDictionary.Keys => this.GetKeyListHelper();

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.GetKeyListHelper();

        public IList<TValue> Values => this.GetValueListHelper();

        ICollection<TValue> IDictionary<TKey, TValue>.Values => this.GetValueListHelper();

        ICollection IDictionary.Values => this.GetValueListHelper();

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this.GetValueListHelper();

        public int Capacity {
            get => this.keys.Length;
            set {
                if (value != this.keys.Length) {
                    if (value < this.size) {
                        throw new ArgumentOutOfRangeException(nameof(value), "ArgumentOutOfRange_SmallCapacity");
                    }

                    if (value > 0) {
                        TKey[] newKeys = new TKey[value];
                        TValue[] newValues = new TValue[value];
                        if (this.size > 0) {
                            Array.Copy(this.keys, 0, newKeys, 0, this.size);
                            Array.Copy(this.values, 0, newValues, 0, this.size);
                        }

                        this.keys = newKeys;
                        this.values = newValues;
                    }
                    else {
                        this.keys = EmptyKeys;
                        this.values = EmptyValues;
                    }
                }
            }
        }

        // Returns the value associated with the given key. If an entry with the
        // given key is not found, the returned value is null.
        //
        public TValue this[TKey key] {
            get {
                int i = this.IndexOfKey(key);
                if (i >= 0)
                    return this.values[i];

                throw new KeyNotFoundException();
            }
            set {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));
                int i = Array.BinarySearch(this.keys, 0, this.size, key, this.comparer);
                if (i >= 0) {
                    this.values[i] = value;
                    this.version++;
                    return;
                }

                this.Insert(~i, key, value);
            }
        }

        Object IDictionary.this[Object key] {
            get {
                if (IsCompatibleKey(key)) {
                    int i = this.IndexOfKey((TKey) key);
                    if (i >= 0) {
                        return this.values[i];
                    }
                }

                return null;
            }
            set {
                if (!IsCompatibleKey(key)) {
                    throw new ArgumentNullException(nameof(key));
                }

                if (value == null && default(TValue) != null) {
                    throw new ArgumentNullException(nameof(value));
                }

                try {
                    TKey tempKey = (TKey) key;
                    try {
                        this[tempKey] = (TValue) value;
                    }
                    catch (InvalidCastException) {
                        throw new ArgumentException("Argument is the wrong type", nameof(value));
                    }
                }
                catch (InvalidCastException) {
                    throw new ArgumentException("Argument is the wrong type", nameof(key));
                }
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        bool IDictionary.IsReadOnly => false;

        bool IDictionary.IsFixedSize => false;

        bool ICollection.IsSynchronized => false;

        // Synchronization root for this object.
        Object ICollection.SyncRoot {
            get {
                if (this._syncRoot == null) {
                    System.Threading.Interlocked.CompareExchange(ref this._syncRoot, new Object(), null);
                }

                return this._syncRoot;
            }
        }

        public DictionaryList() {
            this.keys = EmptyKeys;
            this.values = EmptyValues;
            this.size = 0;
            this.comparer = Comparer<TKey>.Default;
        }

        public void Add(TKey key, TValue value) {
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Key is null");
            int i = Array.BinarySearch(this.keys, 0, this.size, key, this.comparer);
            if (i >= 0)
                throw new InvalidOperationException("Duplicate key");
            this.Insert(~i, key, value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair) {
            this.Add(keyValuePair.Key, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair) {
            int index = this.IndexOfKey(keyValuePair.Key);
            return index >= 0 && EqualityComparer<TValue>.Default.Equals(this.values[index], keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair) {
            int index = this.IndexOfKey(keyValuePair.Key);
            if (index >= 0 && EqualityComparer<TValue>.Default.Equals(this.values[index], keyValuePair.Value)) {
                this.RemoveAt(index);
                return true;
            }

            return false;
        }

        void IDictionary.Add(Object key, Object value) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null && default(TValue) != null) {
                throw new ArgumentNullException(nameof(value));
            }

            try {
                TKey tempKey = (TKey) key;

                try {
                    this.Add(tempKey, (TValue) value);
                }
                catch (InvalidCastException) {
                    throw new ArgumentException("Argument is the wrong type", nameof(value));
                }
            }
            catch (InvalidCastException) {
                throw new ArgumentException("Argument is the wrong type", nameof(key));
            }
        }

        public IList<TKey> Keys => this.GetKeyListHelper();

        private KeyList GetKeyListHelper() {
            return this.keyList ?? (this.keyList = new KeyList(this));
        }

        private ValueList GetValueListHelper() {
            return this.valueList ?? (this.valueList = new ValueList(this));
        }

        public void Clear() {
            this.version++;
            // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
            Array.Clear(this.keys, 0, this.size);
            Array.Clear(this.values, 0, this.size);
            this.size = 0;
        }


        bool IDictionary.Contains(Object key) {
            return IsCompatibleKey(key) && this.ContainsKey((TKey) key);
        }

        public bool ContainsKey(TKey key) {
            return this.IndexOfKey(key) >= 0;
        }

        public bool ContainsValue(TValue value) {
            return this.IndexOfValue(value) >= 0;
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "ArgumentOutOfRange_NeedNonNegNum");
            if (array.Length - arrayIndex < this.Count)
                throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
            for (int i = 0; i < this.Count; i++) {
                KeyValuePair<TKey, TValue> entry = new KeyValuePair<TKey, TValue>(this.keys[i], this.values[i]);
                array[arrayIndex + i] = entry;
            }
        }

        void ICollection.CopyTo(Array array, int arrayIndex) {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (array.Rank != 1)
                throw new ArgumentException("Arg_RankMultiDimNotSupported");
            if (array.GetLowerBound(0) != 0)
                throw new ArgumentException("Arg_NonZeroLowerBound");
            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "ArgumentOutOfRange_NeedNonNegNum");
            if (array.Length - arrayIndex < this.Count)
                throw new ArgumentException("Arg_ArrayPlusOffTooSmall");

            if (array is KeyValuePair<TKey, TValue>[] kvpArray) {
                for (int i = 0; i < this.Count; i++) {
                    kvpArray[i + arrayIndex] = new KeyValuePair<TKey, TValue>(this.keys[i], this.values[i]);
                }
            }
            else if (array is object[] objects) {
                try {
                    for (int i = 0; i < this.Count; i++) {
                        objects[i + arrayIndex] = new KeyValuePair<TKey, TValue>(this.keys[i], this.values[i]);
                    }
                }
                catch (ArrayTypeMismatchException) {
                    throw new ArgumentException("Invalid array storage type");
                }
            }
            else {
                throw new ArgumentException("Invalid array storage type");
            }
        }

        private void EnsureCapacity(int min) {
            int newCapacity = this.keys.Length == 0 ? DefaultCapacity : this.keys.Length * 2;
            // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint) newCapacity > MaxArrayLength)
                newCapacity = MaxArrayLength;
            if (newCapacity < min)
                newCapacity = min;
            this.Capacity = newCapacity;
        }

        private TValue GetByIndex(int index) {
            if (index < 0 || index >= this.size)
                throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_Index");
            return this.values[index];
        }

        private TKey GetKey(int index) {
            if (index < 0 || index >= this.size)
                throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_Index");
            return this.keys[index];
        }

        // Returns the index of the entry with a given key in this sorted list. The
        // key is located through a binary search, and thus the average execution
        // time of this method is proportional to Log2(size), where
        // size is the size of this sorted list. The returned value is -1 if
        // the given key does not occur in this sorted list. Null is an invalid
        // key value.
        //
        public int IndexOfKey(TKey key) {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            int ret = Array.BinarySearch(this.keys, 0, this.size, key, this.comparer);
            return ret >= 0 ? ret : -1;
        }

        // Returns the index of the first occurrence of an entry with a given value
        // in this sorted list. The entry is located through a linear search, and
        // thus the average execution time of this method is proportional to the
        // size of this sorted list. The elements of the list are compared to the
        // given value using the Object.Equals method.
        //
        public int IndexOfValue(TValue value) {
            return Array.IndexOf(this.values, value, 0, this.size);
        }

        // Inserts an entry with a given key and value at a given index.
        private void Insert(int index, TKey key, TValue value) {
            if (this.size == this.keys.Length)
                this.EnsureCapacity(this.size + 1);
            if (index < this.size) {
                Array.Copy(this.keys, index, this.keys, index + 1, this.size - index);
                Array.Copy(this.values, index, this.values, index + 1, this.size - index);
            }

            this.keys[index] = key;
            this.values[index] = value;
            this.size++;
            this.version++;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            int i = this.IndexOfKey(key);
            if (i >= 0) {
                value = this.values[i];
                return true;
            }

            value = default;
            return false;
        }

        public void RemoveAt(int index) {
            if (index < 0 || index >= this.size) {
                throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_Index");
            }

            this.size--;
            if (index < this.size) {
                Array.Copy(this.keys, index + 1, this.keys, index, this.size - index);
                Array.Copy(this.values, index + 1, this.values, index, this.size - index);
            }

            this.keys[this.size] = default;
            this.values[this.size] = default;
            this.version++;
        }

        public bool Remove(TKey key) {
            int i = this.IndexOfKey(key);
            if (i >= 0)
                this.RemoveAt(i);
            return i >= 0;
        }

        void IDictionary.Remove(Object key) {
            if (IsCompatibleKey(key)) {
                this.Remove((TKey) key);
            }
        }

        public void TrimExcess() {
            int threshold = (int) (this.keys.Length * 0.9);
            if (this.size < threshold) {
                this.Capacity = this.size;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return new Enumerator(this, Enumerator.DictEntry);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        private static bool IsCompatibleKey(object key) => key != null ? key is TKey : throw new ArgumentNullException(nameof(key));

        private struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator {
            private readonly DictionaryList<TKey, TValue> _sortedList;
            private TKey key;
            private TValue value;
            private int index;
            private readonly int version;
            private readonly int getEnumeratorRetType; // What should Enumerator.Current return?

            internal const int KeyValuePair = 1;
            internal const int DictEntry = 2;

            DictionaryEntry IDictionaryEnumerator.Entry {
                get {
                    if (this.index == 0 || (this.index == this._sortedList.Count + 1))
                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    return new DictionaryEntry(this.key, this.value);
                }
            }

            public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(this.key, this.value);

            Object IEnumerator.Current {
                get {
                    if (this.index == 0 || (this.index == this._sortedList.Count + 1))
                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    if (this.getEnumeratorRetType == DictEntry) {
                        return new DictionaryEntry(this.key, this.value);
                    }
                    else {
                        return new KeyValuePair<TKey, TValue>(this.key, this.value);
                    }
                }
            }

            Object IDictionaryEnumerator.Value {
                get {
                    if (this.index == 0 || (this.index == this._sortedList.Count + 1))
                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    return this.value;
                }
            }

            Object IDictionaryEnumerator.Key {
                get {
                    if (this.index == 0 || (this.index == this._sortedList.Count + 1))
                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    return this.key;
                }
            }

            internal Enumerator(DictionaryList<TKey, TValue> sortedList, int getEnumeratorRetType) {
                this._sortedList = sortedList;
                this.index = 0;
                this.version = this._sortedList.version;
                this.getEnumeratorRetType = getEnumeratorRetType;
                this.key = default;
                this.value = default;
            }

            public void Dispose() {
                this.index = 0;
                this.key = default;
                this.value = default;
            }

            public bool MoveNext() {
                if (this.version != this._sortedList.version)
                    throw new InvalidOperationException("Concurrent modification");

                if ((uint) this.index < (uint) this._sortedList.Count) {
                    this.key = this._sortedList.keys[this.index];
                    this.value = this._sortedList.values[this.index];
                    this.index++;
                    return true;
                }

                this.index = this._sortedList.Count + 1;
                this.key = default;
                this.value = default;
                return false;
            }

            void IEnumerator.Reset() {
                if (this.version != this._sortedList.version) {
                    throw new InvalidOperationException("Concurrent modification");
                }

                this.index = 0;
                this.key = default;
                this.value = default;
            }
        }

        private sealed class SortedListKeyEnumerator : IEnumerator<TKey>, IEnumerator {
            private DictionaryList<TKey, TValue> _sortedList;
            private int index;
            private int version;
            private TKey currentKey;

            public TKey Current => this.currentKey;

            Object IEnumerator.Current {
                get {
                    if (this.index == 0 || (this.index == this._sortedList.Count + 1))
                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    return this.currentKey;
                }
            }

            internal SortedListKeyEnumerator(DictionaryList<TKey, TValue> sortedList) {
                this._sortedList = sortedList;
                this.version = sortedList.version;
            }

            public void Dispose() {
                this.index = 0;
                this.currentKey = default;
            }

            public bool MoveNext() {
                if (this.version != this._sortedList.version) {
                    throw new InvalidOperationException("Concurrent modification");
                }

                if ((uint) this.index < (uint) this._sortedList.Count) {
                    this.currentKey = this._sortedList.keys[this.index];
                    this.index++;
                    return true;
                }

                this.index = this._sortedList.Count + 1;
                this.currentKey = default;
                return false;
            }

            void IEnumerator.Reset() {
                if (this.version != this._sortedList.version) {
                    throw new InvalidOperationException("Concurrent modification");
                }

                this.index = 0;
                this.currentKey = default;
            }
        }

        private sealed class SortedListValueEnumerator : IEnumerator<TValue>, IEnumerator {
            private readonly DictionaryList<TKey, TValue> _sortedList;
            private int index;
            private readonly int version;
            private TValue currentValue;

            public TValue Current => this.currentValue;

            Object IEnumerator.Current {
                get {
                    if (this.index == 0 || (this.index == this._sortedList.Count + 1))
                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    return this.currentValue;
                }
            }

            internal SortedListValueEnumerator(DictionaryList<TKey, TValue> sortedList) {
                this._sortedList = sortedList;
                this.version = sortedList.version;
            }

            public void Dispose() {
                this.index = 0;
                this.currentValue = default;
            }

            public bool MoveNext() {
                if (this.version != this._sortedList.version) {
                    throw new InvalidOperationException("Concurrent modification");
                }

                if ((uint) this.index < (uint) this._sortedList.Count) {
                    this.currentValue = this._sortedList.values[this.index];
                    this.index++;
                    return true;
                }

                this.index = this._sortedList.Count + 1;
                this.currentValue = default;
                return false;
            }

            void IEnumerator.Reset() {
                if (this.version != this._sortedList.version) {
                    throw new InvalidOperationException("Concurrent modification");
                }

                this.index = 0;
                this.currentValue = default;
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        private sealed class KeyList : IList<TKey>, ICollection {
            private readonly DictionaryList<TKey, TValue> _dict;

            public int Count => this._dict.size;

            public bool IsReadOnly => true;

            bool ICollection.IsSynchronized => false;

            Object ICollection.SyncRoot => ((ICollection) this._dict).SyncRoot;

            internal KeyList(DictionaryList<TKey, TValue> dictionary) {
                this._dict = dictionary;
            }

            public void Add(TKey key) => throw new InvalidOperationException("Cannot write/modify a nested class");

            public void Clear() => throw new InvalidOperationException("Cannot write/modify a nested class");

            public bool Contains(TKey key) {
                return this._dict.ContainsKey(key);
            }

            public void CopyTo(TKey[] array, int arrayIndex) {
                // defer error checking to Array.Copy
                Array.Copy(this._dict.keys, 0, array, arrayIndex, this._dict.Count);
            }

            void ICollection.CopyTo(Array array, int arrayIndex) {
                if (array != null && array.Rank != 1)
                    throw new ArgumentException("Arg_RankMultiDimNotSupported");

                try {
                    // defer error checking to Array.Copy
                    Array.Copy(this._dict.keys, 0, array, arrayIndex, this._dict.Count);
                }
                catch (ArrayTypeMismatchException) {
                    throw new ArgumentException("Invalid array storage type");
                }
            }

            public void Insert(int index, TKey value) {
                throw new InvalidOperationException("Cannot write/modify a nested class");
            }

            public TKey this[int index] {
                get {
                    return this._dict.GetKey(index);
                }
                set {
                    throw new InvalidOperationException("NotSupported_KeyCollectionSet");
                }
            }

            public IEnumerator<TKey> GetEnumerator() => new SortedListKeyEnumerator(this._dict);

            IEnumerator IEnumerable.GetEnumerator() => new SortedListKeyEnumerator(this._dict);

            public int IndexOf(TKey key) {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));
                int i = Array.BinarySearch(this._dict.keys, 0, this._dict.Count, key, this._dict.comparer);
                return i >= 0 ? i : -1;
            }

            public bool Remove(TKey key) => throw new InvalidOperationException("Cannot write/modify a nested class");

            public void RemoveAt(int index) => throw new InvalidOperationException("Cannot write/modify a nested class");
        }

        [DebuggerDisplay("Count = {Count}")]
        private sealed class ValueList : IList<TValue>, ICollection {
            private readonly DictionaryList<TKey, TValue> dict;

            public int Count => this.dict.size;

            public bool IsReadOnly => true;

            bool ICollection.IsSynchronized => false;

            Object ICollection.SyncRoot => ((ICollection) this.dict).SyncRoot;

            public TValue this[int index] {
                get => this.dict.GetByIndex(index);
                set => throw new InvalidOperationException("Cannot write/modify a nested class");
            }

            internal ValueList(DictionaryList<TKey, TValue> dictionary) {
                this.dict = dictionary;
            }

            public void Add(TValue key) {
                throw new InvalidOperationException("Cannot write/modify a nested class");
            }

            public void Clear() {
                throw new InvalidOperationException("Cannot write/modify a nested class");
            }

            public bool Contains(TValue value) {
                return this.dict.ContainsValue(value);
            }

            public void CopyTo(TValue[] array, int arrayIndex) {
                Array.Copy(this.dict.values, 0, array, arrayIndex, this.dict.Count);
            }

            void ICollection.CopyTo(Array array, int arrayIndex) {
                if (array != null && array.Rank != 1)
                    throw new ArgumentException("Arg_RankMultiDimNotSupported");

                try {
                    Array.Copy(this.dict.values, 0, array, arrayIndex, this.dict.Count);
                }
                catch (ArrayTypeMismatchException) {
                    throw new ArgumentException("Invalid array storage type");
                }
            }

            public void Insert(int index, TValue value) => throw new InvalidOperationException("Cannot write/modify a nested class");

            public IEnumerator<TValue> GetEnumerator() {
                return new SortedListValueEnumerator(this.dict);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return new SortedListValueEnumerator(this.dict);
            }

            public int IndexOf(TValue value) {
                return Array.IndexOf(this.dict.values, value, 0, this.dict.Count);
            }

            public bool Remove(TValue value) => throw new InvalidOperationException("Cannot write/modify a nested class");

            public void RemoveAt(int index) => throw new InvalidOperationException("Cannot write/modify a nested class");
        }
    }
}