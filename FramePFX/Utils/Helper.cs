namespace FramePFX.Utils {
    public static class Helper {
        /// <summary>
        /// Gets the (old) value at location, updates it with the new value, and then returns that old value
        /// </summary>
        /// <param name="location">The field</param>
        /// <param name="newValue">The new value</param>
        /// <typeparam name="T">The type of value</typeparam>
        /// <returns>The old value</returns>
        public static T Exchange<T>(ref T location, T newValue) {
            T value = location;
            location = newValue;
            return value;
        }

        /// <summary>
        /// A convenience function for replacing the <see cref="location"/> ref parameter with <see cref="newValue"/>
        /// (when <see cref="location"/> is non-null) and setting <see cref="oldValue"/> to the previous value of <see cref="location"/>
        /// <para>
        /// If <see cref="location"/> is null, nothing happens and this function returns true
        /// </para>
        /// </summary>
        /// <param name="location">The reference to set to the new value</param>
        /// <param name="newValue">The new value to be set at the location</param>
        /// <param name="oldValue">The previous value of location</param>
        /// <typeparam name="T">The type of value</typeparam>
        /// <returns>True if the location is non-null, otherwise false</returns>
        public static bool Exchange<T>(ref T location, T newValue, out T oldValue) where T : class {
            if ((oldValue = location) == null) {
                return false;
            }
            else {
                location = newValue;
                return true;
            }
        }
    }
}