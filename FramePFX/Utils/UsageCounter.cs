using System;

namespace FramePFX.Utils {
    public class UsageCounter {
        private uint deep;

        public bool IsInUse => this.deep > 0;

        public bool IsFree => this.deep <= 0;

        /// <summary>
        /// Increments the usage counter
        /// </summary>
        /// <returns>
        /// True if it was originally not in use, otherwise false if it was already in use
        /// </returns>
        public bool Use() {
            this.deep++;
            return this.deep == 1;
        }

        /// <summary>
        /// Decrements the counter
        /// </summary>
        /// <returns>
        /// True if there are no more objects in use, otherwise false if there are still usages
        /// </returns>
        public bool Free() {
            if (this.deep == 0) {
                throw new Exception("Too many calls to Free()");
            }

            this.deep--;
            return this.deep == 0;
        }
    }
}