using System;
using System.Collections.Generic;

namespace FramePFX.Utils {
    public static class CollectionUtils {
        public static void Foreach<T>(this IEnumerable<T> enumerable, Action<T> consumer) {
            foreach (T value in enumerable) {
                consumer(value);
            }
        }
    }
}