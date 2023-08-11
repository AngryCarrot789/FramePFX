namespace FramePFX.Core.Configurations.Sections {
    /// <summary>
    /// A configuration section
    /// </summary>
    public interface ISection {
        /// <summary>
        /// This section's parent section, or null if this is the root section
        /// </summary>
        ISection Parent { get; }

        /// <summary>
        /// Get or set the value with the given key
        /// <para>
        /// When getting, if the value does not exist, null is returned
        /// </para>
        /// <para>
        /// When setting, if the value is null, then the entry is removed
        /// </para>
        /// </summary>
        /// <param name="key">The key to use to get or set</param>
        object this[string key] { get; set; }

        /// <summary>
        /// Get a value in the given path, or null if the path does not exist
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        object Get(string path);

        /// <summary>
        /// Sets a value in the given path, and creates any sub-configuration sections if nessesary
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        void Set(string path, object value);

        /// <summary>
        /// Sets a value in the given path, and can optionally create any sub-configuration sections if nessesary
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        bool Set(string path, object value, bool createSubSections = true);

        /// <summary>
        /// Tries to get a value if it is in this section.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGet(string path, out object value);

        /// <summary>
        /// Checks if this section contains a value with the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool Contains(string path);

        /// <summary>
        /// Checks if this section contains a value with the given key
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool ContainsKey(string key);

        /// <summary>
        /// Replaces or inserts the value with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        object Replace(string key, object value);

        int GetInt(string key, int def = 0);
        long GetLong(string key, long def = 0);
        float GetFloat(string key, float def = 0f);
        double GetDouble(string key, double def = 0d);

        ISection GetSection(string key);
        ISection GetOrCreateSection(string key); // overwrites existing value
        ISection CreateSection(string key); // overwrites existing value
    }
}