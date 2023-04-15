using System.Collections.Generic;

namespace SharpPadV2.Core.Utils {
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
    }
}