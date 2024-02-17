using System.Collections.Generic;
using System.Linq;

namespace FramePFX.Interactivity.DataContexts {
    /// <summary>
    /// An implementation of <see cref="IDataContext"/> that is completely empty
    /// </summary>
    public sealed class EmptyContext : IDataContext {
        public static IDataContext Instance { get; } = new EmptyContext();

        public IEnumerable<KeyValuePair<string, object>> Entries { get; } = Enumerable.Empty<KeyValuePair<string, object>>();

        public bool TryGetContext(string key, out object value) {
            value = default;
            return false;
        }

        public bool ContainsKey(DataKey key) => false;
        public bool ContainsKey(string key) => false;
    }
}