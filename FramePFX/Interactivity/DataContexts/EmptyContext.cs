using System.Collections.Generic;
using System.Linq;
using FramePFX.Editors.Timelines.Clips;

namespace FramePFX.Interactivity.DataContexts {
    /// <summary>
    /// An implementation of <see cref="IDataContext"/> that is completely empty
    /// </summary>
    public sealed class EmptyContext : IDataContext {
        public static IDataContext Instance { get; } = new EmptyContext();

        public IEnumerable<KeyValuePair<DataKey, object>> Entries { get; } = Enumerable.Empty<KeyValuePair<DataKey, object>>();

        public bool TryGetContext<T>(DataKey<T> key, out T value) {
            value = default;
            return false;
        }

        public bool ContainsKey(DataKey key) => false;

        public bool HasFlag(DataKey<bool> key) => false;
    }
}