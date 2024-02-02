namespace FramePFX.PropertyEditing {
    /// <summary>
    /// An enum that represents the visual group type
    /// </summary>
    public enum GroupType {
        /// <summary>
        /// This is a primary group; it has a big and bold expander
        /// </summary>
        PrimaryExpander,
        /// <summary>
        /// This is a secondary group; it has a small and less obvious expander,
        /// </summary>
        SecondaryExpander,
        /// <summary>
        /// This group has no expander and the contents are always showing (if they are applicable ofc)
        /// </summary>
        NoExpander
    }
}