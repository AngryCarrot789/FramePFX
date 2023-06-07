namespace FramePFX.Core.History {
    public class ValueTransaction<T> {
        /// <summary>
        /// The original value, before any change
        /// </summary>
        public T Original { get; }

        /// <summary>
        /// The current value
        /// </summary>
        public T Current { get; set; }

        public ValueTransaction(T original) {
            this.Original = original;
        }

        public ValueTransaction(T original, T current) : this(original) {
            this.Current = current;
        }

        public void SetCurrent(T current) {
            this.Current = current;
        }
    }
}