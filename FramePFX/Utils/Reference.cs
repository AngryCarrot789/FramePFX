namespace FramePFX.Utils {
    /// <summary>
    /// A class that wraps an object or value type
    /// </summary>
    /// <typeparam name="T">The type of value to wrap</typeparam>
    public class Reference<T> {
        /// <summary>
        /// The value stored in this reference
        /// </summary>
        public T Value;

        public Reference(T value) {
            this.Value = value;
        }

        public Reference() {

        }
    }
}