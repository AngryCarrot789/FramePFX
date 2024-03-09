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

namespace FramePFX.Utils
{
    public static class CollectionUtils
    {
        public static bool CollectionEquals(this IEnumerable a, IEnumerable b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            else if (a == null || b == null)
            {
                return false;
            }
            else if (a is IList listA && b is IList listB)
            {
                return CollectionEquals(listA, listB);
            }
            else if (a is ICollection cA && b is ICollection cB)
            {
                return CollectionEquals(cA, cB);
            }
            else
            {
                return EnumeratorsEquals(a.GetEnumerator(), b.GetEnumerator());
            }
        }

        public static bool CollectionEquals(this IList a, IList b)
        {
            if (ReferenceEquals(a, b))
                return true;
            else if (a == null || b == null)
                return false;
            else if (a.Count != b.Count)
                return false;

            int count = a.Count;
            if (count < 1)
            {
                return true;
            }

            for (int i = 0; i < count; i++)
            {
                if (!Equals(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CollectionEquals(this ICollection a, ICollection b)
        {
            if (ReferenceEquals(a, b))
                return true;
            else if (a == null || b == null)
                return false;
            else if (a.Count != b.Count)
                return false;

            int count = a.Count;
            if (count < 1)
            {
                return true;
            }

            IEnumerator enumA = a.GetEnumerator();
            IEnumerator enumB = b.GetEnumerator();
            try
            {
                for (int i = 0; i < count; i++)
                {
                    if (!enumA.MoveNext() || !enumB.MoveNext())
                    {
                        return false;
                    }

                    if (!Equals(enumA.Current, enumB.Current))
                    {
                        return false;
                    }
                }

                return true;
            }
            finally
            {
                if (enumA is IDisposable)
                    ((IDisposable) enumA).Dispose();
                if (enumB is IDisposable)
                    ((IDisposable) enumB).Dispose();
            }
        }

        public static bool EnumeratorsEquals(this IEnumerator a, IEnumerator b, bool disposeA = true, bool disposeB = true)
        {
            try
            {
                if (ReferenceEquals(a, b))
                    return true;
                else if (a == null || b == null)
                    return false;

                while (true)
                {
                    if (!a.MoveNext())
                    {
                        return !b.MoveNext();
                    }
                    else if (!b.MoveNext())
                    {
                        return false;
                    }
                    else if (!Equals(a.Current, b.Current))
                    {
                        return false;
                    }
                }
            }
            finally
            {
                if (disposeA && a is IDisposable)
                {
                    ((IDisposable) a).Dispose();
                }

                if (disposeB && b is IDisposable)
                {
                    ((IDisposable) b).Dispose();
                }
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> consumer)
        {
            foreach (T value in enumerable)
            {
                consumer(value);
            }
        }

        public static void ForEach<T>(this IEnumerable enumerable, Action<T> consumer)
        {
            foreach (object value in enumerable)
            {
                if (value is T t)
                {
                    consumer(t);
                }
            }
        }

        public static void ForEach(this IEnumerable enumerable, Action<object> consumer)
        {
            foreach (object value in enumerable)
            {
                consumer(value);
            }
        }

        public static void ForEachThenClear<T>(this ICollection<T> list, Action<T> consumer)
        {
            using (ErrorList stack = new ErrorList("An exception occurred while enumerating one or more items before clearing the collection"))
            {
                int i = 0;
                foreach (T item in list)
                {
                    try
                    {
                        consumer(item);
                    }
                    catch (Exception e)
                    {
                        stack.Add(new Exception($"Failed to dispose Item[{i}]", e));
                    }

                    i++;
                }

                list.Clear();
            }
        }

        public static void ClearAndAdd<T>(this ICollection<T> list, T value)
        {
            list.Clear();
            list.Add(value);
        }

        public static void AddCollectionRange<T>(this ICollection<T> list, IEnumerable<T> values)
        {
            foreach (T t in values)
            {
                list.Add(t);
            }
        }

        public static void EnsureLength<T>(T[] array, int count)
        {
            if (array == null || array.Length != count)
            {
                throw new Exception("Expected an array of size " + count + ". Got: " + (array != null ? array.Length.ToString() : "null"));
            }
        }

        public static void MoveItem<T>(this IList<T> list, int oldIndex, int newIndex)
        {
            if (newIndex < 0 || newIndex >= list.Count)
                throw new IndexOutOfRangeException($"{nameof(newIndex)} is not within range: {(newIndex < 0 ? "less than zero" : "greater than list length")} ({newIndex})");
            T removedItem = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, removedItem);
        }

        public static void MoveItem(IList list, int oldIndex, int newIndex)
        {
            object removedItem = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, removedItem);
        }

        public static int IndexOf<T>(this IReadOnlyList<T> list, T value)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < list.Count; i++)
            {
                if (comparer.Equals(value, list[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool Contains<T>(this IReadOnlyList<T> list, T value)
        {
            return IndexOf(list, value) != -1;
        }

        public static SingletonList<T> Singleton<T>(in T value)
        {
            return new SingletonList<T>(value);
        }

        public static List<T> SingleItem<T>(in T value)
        {
            return new List<T>() {value};
        }

        public static int CountAll<T>(this IEnumerable<T> source, Func<T, int> func)
        {
            int count = 0;
            foreach (T value in source)
                count += func(value);
            return count;
        }

        public static bool HasAtleast<T>(this IEnumerable<T> source, int count)
        {
            int i = 0;
            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (++i >= count)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static int GetSortInsertionIndex<T>(IList<T> list, T item, Comparison<T> comparer)
        {
            int left = 0;
            int right = list.Count - 1;
            while (left <= right)
            {
                int middle = left + (right - left) / 2;
                int comparison = comparer(item, list[middle]);
                if (comparison < 0)
                    right = middle - 1;
                else if (comparison > 0)
                    left = middle + 1;
                else
                    return middle;
            }

            return left;
        }

        public static int GetSortInsertionIndex<T>(T[] array, int left, int right, T item, Comparison<T> comparer)
        {
            while (left <= right)
            {
                int middle = left + (right - left) / 2;
                int comparison = comparer(item, array[middle]);
                if (comparison < 0)
                    right = middle - 1;
                else if (comparison > 0)
                    left = middle + 1;
                else
                    return middle;
            }

            return left;
        }

        public static T[] AddToArray<T>(this T[] array, T value, int additionalLength = 1)
        {
            if (additionalLength < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(additionalLength), "Value must be greater than 0");
            }

            int oldLen = array.Length;
            int newLen = oldLen + additionalLength;
            if (additionalLength == 1)
                array = array.CloneArrayMax(newLen);
            array[oldLen + 1] = value;
            return array;
        }

        public static void InsertSorted<T>(this IList<T> list, T value, Comparison<T> comparison)
        {
            int index = GetSortInsertionIndex(list, value, comparison);
            list.Insert(index, value);
        }
    }
}