using System;

namespace FramePFX.Core.ThreadSafety {
    public struct Locker : IDisposable {
        private readonly MutexArray array;

        public Locker(MutexArray array, LockType type) {
            this.array = array;
        }

        public void Dispose() {
        }
    }
}