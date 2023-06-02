using System;
using System.Diagnostics;
using System.Threading;

namespace FramePFX.Core.Utils {
    public sealed class CASLock {
        private readonly object locker;
        private volatile int counter;
        private volatile int isLocking;

        /// <summary>
        /// The amount of locks taken in the current call stack. When this is 0, the lock will be free, allowing other threads to take the lock
        /// </summary>
        public int Count => this.counter;

        private readonly string debugName;

        public CASLock(string debugName = null) {
            this.locker = new object();
            this.debugName = debugName;
        }

        /// <summary>
        /// Attempts to take the lock. When force is true, this function always returns true
        /// </summary>
        /// <param name="force">Whether to force take the lock</param>
        /// <returns>True if the lock was successfully taken or already taken previously</returns>
        public bool Lock(bool force) {
            while (Interlocked.CompareExchange(ref this.isLocking, 1, 0) != 0) {
                Thread.SpinWait(8);
            }

            bool taken = Monitor.TryEnter(this.locker);
            if (!taken) {
                if (force) {
                    Monitor.Enter(this.locker);
                    taken = true;
                    Debug.WriteLine($"[{this.debugName ?? "CASLock"}] Force entered ({this.counter} deep). Thread = {Thread.CurrentThread.ManagedThreadId}");
                }
                else {
                    this.isLocking = 0;
                }
            }
            else {
                Debug.WriteLine($"[{this.debugName ?? "CASLock"}] Entered ({this.counter} deep). Thread = {Thread.CurrentThread.ManagedThreadId}");
            }

            Interlocked.Increment(ref this.counter);
            this.isLocking = 0;
            return taken;
        }

        /// <summary>
        /// Unlocks this <see cref="CASLock"/>. If this function is called before <see cref="Lock"/>, it may corrupt the state of this <see cref="CASLock"/>
        /// </summary>
        public void Unlock() {
            int value = Interlocked.Decrement(ref this.counter);
            Monitor.Exit(this.locker);
            if (value < 1) {
                if (value < 0) {
                    this.counter = 1;
                    throw new Exception("Excess calls to Unlock");
                }

                Debug.WriteLine($"[{this.debugName ?? "CASLock"}] Fully Exited. Thread = {Thread.CurrentThread.ManagedThreadId}");
            }
            else {
                Debug.WriteLine($"[{this.debugName ?? "CASLock"}] Semaphore decremented (not exited). Thread = {Thread.CurrentThread.ManagedThreadId}");
            }
        }
    }
}