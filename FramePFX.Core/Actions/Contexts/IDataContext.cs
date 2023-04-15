using System.Collections.Generic;

namespace FramePFX.Core.Actions.Contexts {
    /// <summary>
    /// An immutable object that stores context information, along other custom data
    /// </summary>
    public interface IDataContext {
        /// <summary>
        /// Returns all of the available context
        /// </summary>
        IEnumerable<object> Context { get; }

        /// <summary>
        /// Returns all of the custom data
        /// </summary>
        IEnumerable<(string, object)> CustomData { get; }

        /// <summary>
        /// Gets a context object of a specific type
        /// </summary>
        T GetContext<T>();

        /// <summary>
        /// Tries to get a context object of the specific type
        /// </summary>
        bool TryGetContext<T>(out T value);

        /// <summary>
        /// Gets custom data for the given key
        /// </summary>
        T Get<T>(string key);

        /// <summary>
        /// Tries to get custom data with the given key and of the given type
        /// </summary>
        bool TryGet<T>(string key, out T value);
    }
}