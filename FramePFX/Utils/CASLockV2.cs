using System.Threading;

namespace FramePFX.Utils {
    public sealed class CASLockV2 {
        private readonly object locker;
        private volatile int counter;

        /// <summary>
        /// The amount of locks taken in the current call stack. When this is 0, the lock is released allowing other threads to take the lock
        /// </summary>
        public int Count => this.counter;

        public CASLockV2() {
            this.locker = new object();
        }

        // public bool Lock(bool force, out bool lockTaken) {
        //     if (Monitor.IsEntered(this.locker)) {
        //         lockTaken = false;
        //     }
        //     else if (Monitor.TryEnter(this.locker)) {
        //         lockTaken = true;
        //     }
        //     else if (force) {
        //         bool taken = false;
        //         Monitor.Enter(this.locker, ref taken);
        //         lockTaken = taken;
        //     }
        //     else {
        //         lockTaken = false;
        //         return false;
        //     }
        //     Interlocked.Increment(ref this.counter);
        //     return true;
        // }

        /// <summary>
        /// Attempts to take the lock. When force is true, this function always returns true
        /// </summary>
        /// <param name="force">Whether to force take the lock</param>
        /// <returns>True if the lock was successfully taken or already taken previously</returns>
        public bool Lock(bool force) {
            if (!Monitor.IsEntered(this.locker) && !Monitor.TryEnter(this.locker)) {
                if (force) {
                    Monitor.Enter(this.locker);
                }
                else {
                    return false;
                }
            }

            Interlocked.Increment(ref this.counter);
            return true;
        }

        /// <summary>
        /// Unlocks this <see cref="CASLockV2"/>. If this function is called before <see cref="Lock"/>, it may corrupt the state of this <see cref="CASLockV2"/>
        /// </summary>
        public void Unlock() {
            if (Interlocked.Decrement(ref this.counter) <= 0) {
                Monitor.Exit(this.locker);
            }
        }
    }
}