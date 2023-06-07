namespace FramePFX.Core.History {
    public class Transactions {
        /// <summary>
        /// Creates a transaction that uses the given value as the original and current,
        /// assuming they're immutable classes or immutable struct types
        /// </summary>
        /// <param name="original"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Transaction<T> Immutable<T>(T original) {
            return new Transaction<T>(original, original);
        }
    }
}