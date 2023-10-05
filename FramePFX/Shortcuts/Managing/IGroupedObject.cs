namespace FramePFX.Shortcuts.Managing {
    /// <summary>
    /// An interface implemented by <see cref="ShortcutGroup"/>, <see cref="GroupedShortcut"/> and <see cref="GroupedInputState"/>
    /// </summary>
    public interface IGroupedObject {
        /// <summary>
        /// Gets the manager that this object belongs to. This typically is equal to <see cref="ShortcutManager.Instance"/>
        /// </summary>
        ShortcutManager Manager { get; }

        /// <summary>
        /// Gets the group that contains this object. Null means that this object is the
        /// root <see cref="ShortcutGroup"/> for a <see cref="ShortcutManager"/>
        /// </summary>
        ShortcutGroup Parent { get; }

        /// <summary>
        /// Gets the name of this grouped object. If this instance is a <see cref="ShortcutGroup"/> and is the root
        /// for a <see cref="ShortcutManager"/>, then this value will be null. Otherwise, This will not be null,
        /// empty or consist of only whitespaces; it will always be a valid string (even if only 1 character)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the full path of this grouped object, which combines the parent (<see cref="Parent"/>)'s <see cref="FullPath"/> and
        /// the current instance's <see cref="Name"/> with a '/' character. If the parent is null, this is equal to <see cref="Name"/>.
        /// <para>
        /// As per the docs for <see cref="Name"/>, it will always be a valid string (with at least 1 character)
        /// </para>
        /// </summary>
        string FullPath { get; }
    }
}