namespace FramePFX.Utils {
    public static class Helper {
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
        /// <param name="location"></param>
        /// <param name="newValue"></param>
        /// <param name="oldValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>True if <see cref="location"/> is non-null, otherwise false</returns>
        public static bool Exchange<T>(ref T location, T newValue, out T oldValue) where T : class {
            T value = location;
            if (value == null) {
                oldValue = null;
                return false;
            }

            oldValue = value;
            location = newValue;
            return true;
        }
    }
}