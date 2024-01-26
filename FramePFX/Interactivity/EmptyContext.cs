using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FramePFX.Interactivity {
    /// <summary>
    /// An implementation of <see cref="IDataContext"/> that is completely empty
    /// </summary>
    public class EmptyContext : IDataContext {
        private static readonly ReadOnlyCollection<object> EMPTY = new List<object>().AsReadOnly();

        public IReadOnlyList<object> Context { get; } = EMPTY;

        public IEnumerable<(string, object)> Entries { get; } = Enumerable.Empty<(string, object)>();

        public static EmptyContext Instance { get; } = new EmptyContext();

        public EmptyContext() {
        }

        public T GetContext<T>() => default;

        public bool TryGetContext<T>(out T value) {
            value = default;
            return false;
        }

        public bool TryGetContext(Type type, out object value) {
            value = default;
            return false;
        }

        public bool HasContext<T>() => false;
        public bool Contains(object context) => false;

        public T Get<T>(string key) => default;

        public bool TryGet<T>(string key, out T value) {
            value = default;
            return false;
        }

        public bool ContainsKey(string key) => false;

        public bool HasFlag(string key) => false;
    }
}