using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FramePFX.Utils {
    /// <summary>
    /// A list that delegates get/set to a list with a lower type that the required type
    /// </summary>
    public class CastingList<T> : IReadOnlyList<T> {
        private readonly IReadOnlyList<object> list;

        public int Count => this.list.Count;

        public T this[int index] => (T) this.list[index];

        public CastingList(IReadOnlyList<object> list) {
            this.list = list ?? throw new ArgumentNullException(nameof(list));
        }

        public IEnumerator<T> GetEnumerator() {
            return this.list.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.list.Cast<T>().GetEnumerator();
        }
    }
}