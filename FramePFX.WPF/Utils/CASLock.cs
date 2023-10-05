using System.Threading;

namespace FramePFX.WPF.Utils {
    public sealed class CASLock {
        private readonly object locker;
        private volatile int counter;

        /// <summary>
        /// The amount of locks taken in the current call stack. When this is 0, the lock will be free, allowing other threads to take the lock
        /// </summary>
        public int Count => this.counter;

        public CASLock() {
            this.locker = new object();
        }

        /// <summary>
        /// Attempts to take the lock. When force is true, this function always returns true
        /// </summary>
        /// <param name="force">Whether to force take the lock</param>
        /// <returns>True if the lock was successfully taken or already taken previously</returns>
        public bool Lock(bool force) {
            bool lockTaken = false;
            Monitor.TryEnter(this.locker, ref lockTaken);
            if (!lockTaken && !Monitor.IsEntered(this.locker)) {
                if (force) {
                    Monitor.Enter(this.locker);
                }
                else {
                    return false;
                }
            }

            Interlocked.Increment(ref this.counter);
            return true;

            // if (!Monitor.IsEntered(this.locker) && !Monitor.TryEnter(this.locker)) {
            //     if (force) {
            //         Monitor.Enter(this.locker);
            //     }
            //     else {
            //         return false;
            //     }
            // }
            // Interlocked.Increment(ref this.counter);
            // return true;
        }

        /// <summary>
        /// Unlocks this <see cref="CASLock"/>. If this function is called before <see cref="Lock"/>, it may corrupt the state of this <see cref="CASLock"/>
        /// </summary>
        public void Unlock() {
            Interlocked.Decrement(ref this.counter);
            Monitor.Exit(this.locker);
        }
    }
}