using System;
using System.Collections.Generic;

namespace FramePFX.History {
    public class Transactions {
        /// <summary>
        /// Creates a transaction that uses the given value as the original and current,
        /// assuming they're immutable classes or immutable struct types
        /// </summary>
        /// <param name="original"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Transaction<T> ImmutableType<T>(T original) => new Transaction<T>(original, original);

        public static Transaction<T> ForBoth<T>(T value) => new Transaction<T>(value, value);

        public static Transaction<T>[] NewArray<TSrc, T>(IReadOnlyList<TSrc> sources, Func<TSrc, T> getter) where TSrc : class, IHistoryHolder {
            Transaction<T>[] array = new Transaction<T>[sources.Count];
            for (int i = 0; i < array.Length; i++)
                array[i] = ForBoth(getter(sources[i]));
            return array;
        }
    }
}