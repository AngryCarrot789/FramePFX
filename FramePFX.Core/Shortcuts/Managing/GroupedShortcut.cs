using System;
using System.Text;

namespace FramePFX.Core.Shortcuts.Managing {
    /// <summary>
    /// A class used to store a reference to a <see cref="Shortcut"/> and its
    /// owning <see cref="ShortcutGroup"/>, and also other shortcut data
    /// </summary>
    public sealed class GroupedShortcut {
        private IShortcut shortcut;

        /// <summary>
        /// The collection that owns this managed shortcut
        /// </summary>
        public ShortcutGroup Group { get; }

        /// <summary>
        /// The shortcut itself. Will not be null
        /// </summary>
        public IShortcut Shortcut {
            get => this.shortcut;
            set => this.shortcut = value ?? throw new ArgumentNullException(nameof(value), "Shortcut cannot be null");
        }

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
        /// Whether this shortcut is inherited by child shortcuts of the group that owns this shortcut
        /// <para>
        /// Say for instance, this shortcut's path is App/Panel1/MyShortcut1 and it allows inheritance, it's owning group path is therefore App/Panel1. That means that,
        /// if there
        /// </para>
        /// </summary>
        public bool Inherit { get; set; }

        /// <summary>
        /// The ID for an optional action that this shortcut will trigger when activated
        /// </summary>
        public string ActionId { get; set; }

        /// <summary>
        /// This shortcut's full path (the parent's path (if available/not root) and this shortcut's name)
        /// </summary>
        public string Path { get; }

        public GroupedShortcut(ShortcutGroup @group, string name, IShortcut shortcut, bool isGlobal = false, bool inherit = false) {
            this.Group = @group ?? throw new ArgumentNullException(nameof(@group), "Collection cannot be null");
            this.Shortcut = shortcut;
            this.Path = @group.GetPathForName(name);
            this.Name = name;
            this.IsGlobal = isGlobal;
            this.Inherit = inherit;
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