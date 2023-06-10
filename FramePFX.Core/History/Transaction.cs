namespace FramePFX.Core.History {
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

        /// <summary>
        /// A function that just sets <see cref="Current"/>
        /// </summary>
        /// <param name="current">New current value</param>
        public void SetCurrent(T current) {
            this.Current = current;
        }
    }
}