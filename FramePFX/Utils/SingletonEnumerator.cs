using System.Collections;
using System.Collections.Generic;

namespace FramePFX.Utils {
    public class SingletonEnumerator<T> : IEnumerator<T> {
        private bool hasMovedNext;

        // should this throw if hasMovedNext is false?
        public T Current { get; }

        object IEnumerator.Current => this.Current;

        public SingletonEnumerator(T value) {
            this.Current = value;
        }

        public bool MoveNext() {
            if (this.hasMovedNext) {
                return false;
            }
            else {
                this.hasMovedNext = true;
                return true;
            }
        }

        public void Reset() {
            this.hasMovedNext = false;
        }

        public void Dispose() {
        }
    }
}