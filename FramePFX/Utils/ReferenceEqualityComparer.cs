using System.Collections.Generic;

namespace FramePFX.Utils {
    public class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class {
        private static ReferenceEqualityComparer<T> instance;

        public static ReferenceEqualityComparer<T> Default {
            get => instance ?? (instance = new ReferenceEqualityComparer<T>());
        }

        public bool Equals(T x, T y) => ReferenceEquals(x, y);

        public int GetHashCode(T obj) => obj.GetHashCode();
    }
}