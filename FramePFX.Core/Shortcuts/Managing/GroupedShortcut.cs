using System;
using System.Text;
using FramePFX.Core.Actions.Contexts;

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
        /// The name of the shortcut. This will not be null or empty and will not consist of only whitespaces;
        /// this is always some sort of valid string (even if only 1 character)
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
        /// Whether this shortcut can be targeted when the focused group is deeper in the focus tree but somewhere up the tree is a group which contains this shortcut
        /// <para>
        /// For example, this shortcut's path is <c>app/main-window/OpenFile</c> and the focused group is <c>app/main-window/content/text-editor</c>, if no
        /// shortcuts in that specific group with this shortcut's input combination exists and no other shortcuts could be found via inheritance apart from
        /// this instance, then this instance can be targeted. The first shortcut to be found via inheritance is the one that gets targeted; others are ignored.
        /// This means the order of shortcuts in the XML document can play a part in which shortcut is finally executed
        /// </para>
        /// </summary>
        public bool IsInherited { get; set; }

        /// <summary>
        /// The ID for an optional action that this shortcut will trigger when activated
        /// </summary>
        public string ActionId { get; set; }

        /// <summary>
        /// This shortcut's full path (the parent's path (if available/not root) and this shortcut's name)
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Additional context for this shortcut to be passed to the action system
        /// </summary>
        public DataContext ActionContext { get; set; }

        public GroupedShortcut(ShortcutGroup @group, string name, IShortcut shortcut, bool isGlobal = false, bool isInherited = false) {
            this.Group = @group ?? throw new ArgumentNullException(nameof(@group), "Collection cannot be null");
            this.Shortcut = shortcut;
            this.FullPath = @group.GetPathForName(name);
            this.Name = name;
            this.IsGlobal = isGlobal;
            this.IsInherited = isInherited;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Shortcut.IsEmpty ? "Empty/No Shortcut" : this.Shortcut.ToString()).Append(" -> ").Append(this.FullPath);
            if (!string.IsNullOrWhiteSpace(this.Description)) {
                sb.Append(" (").Append(this.Description).Append(")");
            }

            return sb.ToString();
        }
    }
}