using System;
using System.Collections;
using System.Collections.Generic;

namespace FramePFX.Utils {
    public class SingletonList<T> : IReadOnlyList<T> {
        private readonly T value;

        public int Count => 1;

        public T this[int index] => index == 0 ? this.value : throw new IndexOutOfRangeException("Index was out of range: " + index);

        public SingletonList(T value) {
            this.value = value;
        }

        public IEnumerator<T> GetEnumerator() {
            yield return this.value;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            yield return this.value;
        }
    }
}