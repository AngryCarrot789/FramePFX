using System.Collections.Generic;

namespace FramePFX.Interactivity.DataContexts {
    /// <summary>
    /// An immutable object that stores context information. Any entry will always have a non-null value; null values are not permitted
    /// </summary>
    public interface IDataContext {
        /// <summary>
        /// Returns all of the custom data. Will not return null, but may be empty
        /// </summary>
        IEnumerable<KeyValuePair<DataKey, object>> Entries { get; }

        /// <summary>
        /// Tries to get a value from a data key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool TryGetContext<T>(DataKey<T> key, out T value);

        /// <summary>
        /// Checks if the given data key is contained in this context
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool ContainsKey(DataKey key);

        /// <summary>
        /// Returns true if the data key exists in this context and has a value of ture
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool HasFlag(DataKey<bool> key);
    }
}