namespace FramePFX.PropertyEditing {
    /// <summary>
    /// A mode for determining if a <see cref="BasePropertyEditorViewModel"/> is applicable for a collection of handlers
    /// </summary>
    public enum ApplicabilityMode {
        /// <summary>
        /// Applicable when all handlers are applicable
        /// </summary>
        All,
        /// <summary>
        /// Applicable when any of the handlers are applicable, and only those handlers will be used
        /// </summary>
        Any
    }
}