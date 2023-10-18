using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FramePFX.Utils.Collections {
    public class ReferenceDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : class {
        public ReferenceDictionary() : base(ReferenceEqualityComparer.Instance) {

        }

        public ReferenceDictionary(int capacity) : base(capacity, ReferenceEqualityComparer.Instance) {

        }

        public ReferenceDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary, ReferenceEqualityComparer.Instance) {

        }

        private class ReferenceEqualityComparer : IEqualityComparer<TKey> {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

            public bool Equals(TKey x, TKey y) => ReferenceEquals(x, y);
            public int GetHashCode(TKey obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}
