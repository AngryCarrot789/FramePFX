using System;
using System.Collections.Generic;

namespace FramePFX.History {
    /// <summary>
    /// A class for storing a transaction of a value; an unchangeable original and a changeable current value
    /// </summary>
    /// <typeparam name="T">The value transaction type</typeparam>
    public class Transaction<T> {
        /// <summary>
        /// The original value, before any change
        /// </summary>
        public T Original { get; }

        /// <summary>
        /// The current value
        /// </summary>
        public T Current { get; set; }

        public Transaction(T original) {
            this.Original = original;
        }

        public Transaction(T original, T current) : this(original) {
            this.Current = current;
        }

        // Convenience function to reduce code duplication

        /// <summary>
        /// Gets the value as either the original or current
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public T GetValue(bool original) => original ? this.Original : this.Current;

        /// <summary>
        /// A function that just sets <see cref="Current"/>
        /// </summary>
        /// <param name="current">New current value</param>
        public void SetCurrent(T current) {
            this.Current = current;
        }

        /// <summary>
        /// Whether or not this transaction's origin and current value are equal (see <see cref="EqualityComparer{T}.Equals(T,T)"/>)
        /// </summary>
        /// <returns></returns>
        public bool IsUnchanged() => EqualityComparer<T>.Default.Equals(this.Original, this.Current);

        public bool IsUnchanged(Func<T, T, bool> equal) => equal(this.Original, this.Current);

        public bool HasChanged() => !this.IsUnchanged();

        public bool HasChanged(Func<T, T, bool> equal) => !this.IsUnchanged(equal);
    }
}