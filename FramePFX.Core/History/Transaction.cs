namespace FramePFX.Core.History {
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

        public void SetCurrent(T current) {
            this.Current = current;
        }
    }
}