using System;
using System.Collections;
using System.Collections.Generic;

namespace FramePFX.Utils {
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
    }
}