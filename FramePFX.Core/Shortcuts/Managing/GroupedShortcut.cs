using System;
using System.Text;

namespace SharpPadV2.Core.Shortcuts.Managing {
    /// <summary>
    /// A class used to store a reference to a <see cref="Shortcut"/> and its
    /// owning <see cref="ShortcutGroup"/>, and also other shortcut data
    /// </summary>
    public sealed class GroupedShortcut {
        /// <summary>
        /// The collection that owns this managed shortcut
        /// </summary>
        public ShortcutGroup Group { get; }

        /// <summary>
        /// The shortcut itself
        /// </summary>
        public IShortcut Shortcut { get; private set; }

        /// <summary>
        /// The name of the shortcut
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// This group's display name, which is a more readable and user-friendly version of <see cref="Name"/>
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// A description of what the shortcut is used for
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Whether this shortcut can run globally (across the entire window). When false, the parent group must be focused in order for this to be fun
        /// </summary>
        public bool IsGlobal { get; set; }

        /// <summary>
        /// The ID for an optional action that this shortcut will trigger when activated
        /// </summary>
        public string ActionId { get; set; }

        /// <summary>
        /// This shortcut's full path (the parent's path (if available/not root) and this shortcut's name)
        /// </summary>
        public string Path { get; }

        public GroupedShortcut(ShortcutGroup collection, string name, IShortcut shortcut, bool isGlobal = false) {
            this.Group = collection ?? throw new ArgumentNullException(nameof(collection), "Collection cannot be null");
            this.Shortcut = shortcut ?? throw new ArgumentNullException(nameof(shortcut), "Shortcut cannot be null");
            this.Path = collection.GetPathForName(name);
            this.Name = name;
            this.IsGlobal = isGlobal;
        }

        public void SetShortcut(IShortcut shortcut) {
            this.Shortcut = shortcut ?? throw new ArgumentNullException(nameof(shortcut), "Shortcut cannot be null");
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Shortcut).Append(" -> ").Append(this.Path);
            if (!string.IsNullOrWhiteSpace(this.Description)) {
                sb.Append(" (").Append(this.Description).Append(")");
            }

            return sb.ToString();
        }
    }
}