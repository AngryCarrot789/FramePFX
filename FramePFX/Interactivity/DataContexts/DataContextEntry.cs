namespace FramePFX.Interactivity.DataContexts {
    public class DataContextEntry {
        /// <summary>
        /// Gets the data key
        /// </summary>
        public DataKey Key { get; }

        /// <summary>
        /// Gets the data value
        /// </summary>
        public object Value { get; }

        public DataContextEntry(DataKey key, object value) {
            this.Key = key;
            this.Value = value;
        }

        public static DataContextEntry Of<T>(DataKey<T> key, T value) => new DataContextEntry(key, value);
    }
}