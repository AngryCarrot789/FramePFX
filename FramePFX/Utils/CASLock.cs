using System.Threading;

namespace FramePFX.Utils {
    public sealed class CASLock {
        private const int Free = 0;
        private const int Used = 1;

        private volatile int state;
        private volatile int thread;

        private bool TryTakeLock() {
            return Interlocked.CompareExchange(ref this.state, Used, Free) == Free;
        }

        public bool TryLock(out CASLockType lockType) {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            if (this.thread == threadId) {
                this.state = Used;
                lockType = CASLockType.Thread;
                return true;
            }

            if (this.TryTakeLock()) {
                this.thread = threadId;
                lockType = CASLockType.WasNotLocked;
                return true;
            }

            lockType = CASLockType.Failed;
            return false;
        }

        /// <summary>
        /// Spin-wait lock
        /// </summary>
        public void Lock(out CASLockType lockType) {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            if (this.thread == threadId) {
                this.state = Used;
                lockType = CASLockType.Thread;
                return;
            }

            lockType = CASLockType.WasNotLocked;
            do {
                if (Interlocked.CompareExchange(ref this.state, Used, Free) == Free) {
                    this.thread = threadId;
                    return;
                }

                lockType = CASLockType.Normal;
                Thread.SpinWait(16);
            } while (true);
        }

        public void LockUnsafe() {
            this.thread = Thread.CurrentThread.ManagedThreadId;
            this.state = Used;
        }

        public void Unlock() {
            this.thread = 0;
            this.state = Free;
        }

        /// <summary>
        /// Tries to safely unlock this <see cref="CASLock"/>. Typically, it will only be unlocked if the type
        /// is not <see cref="Thread"/> (because the thread already locked it, so force unlocking may be unsafe)
        /// </summary>
        /// <param name="type"></param>
        /// <returns>True if unlocked, otherwise false</returns>
        public bool Unlock(CASLockType type) {
            if (type != CASLockType.Thread && type != CASLockType.Failed) {
                this.Unlock();
                return true;
            }

            return false;
        }

        public bool IsLocked() {
            return this.state == Used && Thread.CurrentThread.ManagedThreadId != this.thread;
        }

        public bool IsFree() {
            return this.state == Free || Thread.CurrentThread.ManagedThreadId == this.thread;
        }

        public override string ToString() {
            return $"{nameof(CASLock)} ({(this.state == Free ? "Free" : "Used")}) ({this.thread})";
        }
    }
}