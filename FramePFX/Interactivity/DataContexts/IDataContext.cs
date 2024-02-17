using System.Collections.Generic;

namespace FramePFX.Interactivity.DataContexts {
    /// <summary>
    /// An immutable object that stores context information. Any entry will always have a non-null value; null values are not permitted
    /// </summary>
    public interface IDataContext {
        /// <summary>
        /// Returns all of the custom data. Will not return null, but may be empty
        /// </summary>
        IEnumerable<KeyValuePair<string, object>> Entries { get; }

        /// <summary>
        /// Tries to get a value from a data key
        /// </summary>
        bool TryGetContext(string key, out object value);

        /// <summary>
        /// Checks if the given data key is contained in this context
        /// </summary>
        bool ContainsKey(DataKey key);

        /// <summary>
        /// Checks if the given data key is contained in this context
        /// </summary>
        bool ContainsKey(string key);
    }
}