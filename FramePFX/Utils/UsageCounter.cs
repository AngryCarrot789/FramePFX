using System;
using System.Threading;

namespace FramePFX.Utils {
    public class UsageCounter {
        private volatile int deep;

        public bool IsInUse => this.deep > 0;

        public bool IsFree => this.deep <= 0;

        /// <summary>
        /// Increments the usage counter
        /// </summary>
        /// <returns>
        /// True if it was originally not in use, otherwise false if it was already in use
        /// </returns>
        public bool Use() {
            lock (this) {
                return Interlocked.Increment(ref this.deep) == 1;
            }
        }

        /// <summary>
        /// Decrements the counter
        /// </summary>
        /// <returns>
        /// True if there are no more objects in use, otherwise false if there are still usages
        /// </returns>
        public bool Free() {
            lock (this) {
                if (this.deep == 0) {
                    throw new Exception("Too many calls to Free()");
                }

                return Interlocked.Decrement(ref this.deep) == 0;
            }
        }
    }
}