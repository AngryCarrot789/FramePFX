using System;
using System.Collections;
using System.Collections.Generic;

namespace FramePFX.Utils
{
    public static class CollectionUtils
    {
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
            using (ErrorList stack = new ErrorList())
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

        public static int GetSortInsertionIndex<T>(IReadOnlyList<T> list, T item, Comparison<T> comparer)
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
    }
}