using System;
using System.Collections.Generic;

namespace FramePFX.Interactivity {
    /// <summary>
    /// An immutable object that stores context information, along other custom data
    /// </summary>
    public interface IDataContext {
        /// <summary>
        /// Returns all of the available context. Will not return  null, but may be empty
        /// </summary>
        IReadOnlyList<object> Context { get; }

        /// <summary>
        /// Returns all of the custom data. Will not return null, but may be empty
        /// </summary>
        IEnumerable<(string, object)> Entries { get; }

        /// <summary>
        /// Gets a context object of a specific type
        /// </summary>
        T GetContext<T>();

        /// <summary>
        /// Tries to get a context object of the specific type
        /// </summary>
        bool TryGetContext<T>(out T value);

        /// <summary>
        /// Tries to get a context object of the specific type
        /// </summary>
        bool TryGetContext(Type type, out object value);

        /// <summary>
        /// Returns whether this data context contains an instance of the given type
        /// </summary>
        bool HasContext<T>();

        /// <summary>
        /// Returns whether this data context contains the given context object
        /// </summary>
        /// <param name="context">The object</param>
        /// <returns>A bool</returns>
        bool Contains(object context);

        /// <summary>
        /// Gets custom data for the given key
        /// </summary>
        T Get<T>(string key);

        /// <summary>
        /// Tries to get custom data with the given key and of the given type
        /// </summary>
        bool TryGet<T>(string key, out T value);

        /// <summary>
        /// Whether or not the custom data contains the given key
        /// </summary>
        bool ContainsKey(string key);

        /// <summary>
        /// A helper function for checking if this context contains the given custom key as a boolean, and that boolean is true
        /// </summary>
        bool HasFlag(string key);
    }
}