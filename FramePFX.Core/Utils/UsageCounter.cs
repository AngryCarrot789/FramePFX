using System;
using System.Threading;

namespace FramePFX.Core.Utils {
    public class UsageCounter {
        private volatile int count;

        public bool IsInUse => this.count > 0;

        public bool IsFree => this.count <= 0;

        /// <summary>
        /// Increments the usage counter
        /// </summary>
        /// <returns>
        /// True if it was originally not in use, otherwise false if it was already in use
        /// </returns>
        public bool Increment() {
            int value = Interlocked.Increment(ref this.count);
            return value == 1;
        }

        /// <summary>
        /// Decrements the counter
        /// </summary>
        /// <returns>
        /// True if there are no more objects in use, otherwise false if there are still usages
        /// </returns>
        public bool Decrement() {
            int result = Interlocked.Decrement(ref this.count);
            if (result == 0) {
                return true;
            }
            else if (result < 0) {
                throw new Exception("Too many calls to Decrement()");
            }
            else {
                return false;
            }
        }
    }
}