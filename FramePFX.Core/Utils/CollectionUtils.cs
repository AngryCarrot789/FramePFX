using System;
using System.Collections;
using System.Collections.Generic;

namespace FramePFX.Core.Utils {
    public static class CollectionUtils {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> consumer) {
            foreach (T value in enumerable) {
                consumer(value);
            }
        }

        public static void ForEach<T>(this IEnumerable enumerable, Action<T> consumer) {
            foreach (object value in enumerable) {
                if (value is T t) {
                    consumer(t);
                }
            }
        }

        public static void ForEach(this IEnumerable enumerable, Action<object> consumer) {
            foreach (object value in enumerable) {
                consumer(value);
            }
        }

        public static void EnsureLength<T>(T[] array, int count) {
            if (array == null || array.Length != count) {
                throw new Exception("Expected an array of size " + count + ". Got: " + (array != null ? array.Length.ToString() : "null"));
            }
        }

        public static void MoveItem<T>(this IList<T> list, int oldIndex, int newIndex) {
            T removedItem = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, removedItem);
        }

        public static IEnumerable<T> SingleItem<T>(in T value) {
            return new List<T> {value};
        }
    }
}