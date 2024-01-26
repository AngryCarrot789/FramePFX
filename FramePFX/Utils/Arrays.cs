using System;
using System.Collections.Generic;

namespace FramePFX.Utils {
    /// <summary>
    /// Contains function that apparently aren't in C# but are in java, used for NBT equality testing
    /// </summary>
    public static class Arrays {
        // Using IEqualityComparer + generic functions is easier than having
        // a Hash function for all types of non-struct type arrays

        public static int Hash<T>(T[] array) {
            if (array == null)
                return 0;

            IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
            int result = 1;
            foreach (T t in array) {
                result = 31 * result + comparer.GetHashCode(t);
            }

            return result;
        }

        public static bool Equals<T>(T[] a, T[] b) {
            int length;
            if (a == b)
                return true;
            if (a == null || b == null || b.Length != (length = a.Length))
                return false;

            IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < length; i++) {
                if (!comparer.Equals(a[i], b[i])) {
                    return false;
                }
            }

            return true;
        }

        public static bool Equals<T>(T[] a, T[] b, Func<T, T, bool> equalityFunction) {
            int length;
            if (a == b)
                return true;
            if (a == null || b == null || a.Length != (length = b.Length))
                return false;

            for (int i = 0; i < length; i++) {
                if (!equalityFunction(a[i], b[i])) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a new array and may use unsafe methods of copying data
        /// from the source to destination if the array is large enough
        /// </summary>
        /// <param name="array"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static unsafe T[] CloneArrayUnsafe<T>(this T[] array) where T : unmanaged {
            if (array == null)
                return null;
            int length = array.Length;
            T[] values = new T[length];
            int bytes = sizeof(T) * length;
            if (bytes > 100 || length > 50) {
                // BlockCopy will most likely help out
                Buffer.BlockCopy(array, 0, values, 0, bytes);
            }
            else if (length > 0) {
                for (int i = 0; i < length; i++)
                    values[i] = array[i];
            }

            return values;
        }

        public static T[] CloneArrayMax<T>(this T[] array) => CloneArrayMax(array, array.Length);

        public static T[] CloneArrayMax<T>(this T[] array, int count) {
            if (array == null)
                return null;
            int len = array.Length;
            T[] values = new T[Math.Max(len, count)];
            for (int i = 0; i < len; i++)
                values[i] = array[i];
            return values;
        }

        public static T[] CloneArrayMin<T>(this T[] array, int minCount) {
            if (array == null)
                return null;
            T[] values = new T[minCount];
            for (int i = 0, count = Math.Min(array.Length, minCount); i < count; i++)
                values[i] = array[i];
            return values;
        }
    }
}