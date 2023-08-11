namespace FramePFX.Core {
    /// <summary>
    /// An interface for translating a key (and optionally, string format parameters) into a final string, based on the current language
    /// </summary>
    public interface ITranslator {
        /// <summary>
        /// Gets a string with the given key, or returns the key if no such key exists
        /// </summary>
        /// <param name="key">The translation key</param>
        /// <returns>The translated value, or key if no such translation exists</returns>
        string GetString(string key);

        /// <summary>
        /// Gets a string with the given key, or returns the key if no such key exists
        /// </summary>
        /// <param name="key">The translation key</param>
        /// <param name="formatParams">Parameters passed to <see cref="string.Format(string,object[])"/></param>
        /// <returns>The translated value, or key if no such translation exists</returns>
        string GetString(string key, params object[] formatParams);

        bool TryGetString(out string output, string key);

        bool TryGetString(out string output, string key, params object[] formatParams);
    }
}