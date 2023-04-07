using System;
using System.Collections.Generic;
using FramePFX.Core.Shortcuts.Inputs;
using FramePFX.Core.Shortcuts.Serialization;

namespace FramePFX.Core.Shortcuts.Managing {
    /// <summary>
    /// A collection of shortcuts
    /// </summary>
    public class ShortcutGroup {
        private readonly List<ShortcutGroup> groups;
        private readonly List<ManagedShortcut> shortcuts;
        private readonly Dictionary<string, object> mapToItem;

        public ShortcutGroup Parent { get; }

        /// <summary>
        /// This group's full path (containing the parent's path and this group's name into one).
        /// It will either be null (meaning no parent), or a non-empty string. It will also never consist of only whitespaces
        /// </summary>
        public string FocusGroupPath { get; }

        /// <summary>
        /// This group's name. It will either be null (meaning no parent), or a non-empty string. It will also never consist of only whitespaces
        /// </summary>
        public string FocusGroupName { get; }

        /// <summary>
        /// A description of what this group contains
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Whether this focus group runs globally (across the entire application). Global focus groups contain shortcuts that can be run, irregardless of what group is focused
        /// </summary>
        public bool IsGlobal { get; set; }

        /// <summary>
        /// Inherits shortcuts from the parent group
        /// </summary>
        public bool InheritFromParent { get; }

        /// <summary>
        /// All shortcuts in this focus group
        /// </summary>
        public IEnumerable<ManagedShortcut> Shortcuts => this.shortcuts;

        /// <summary>
        /// All child-groups in this focus group
        /// </summary>
        public IEnumerable<ShortcutGroup> Groups => this.groups;

        public ShortcutGroup(ShortcutGroup parent, string name, bool isGlobal = false, bool inherit = false) {
            this.FocusGroupPath = (parent != null && name != null) ? parent.GetPathForName(name) : name;
            this.FocusGroupName = name;
            this.InheritFromParent = inherit;
            this.IsGlobal = isGlobal;
            this.Parent = parent;
            this.groups = new List<ShortcutGroup>();
            this.shortcuts = new List<ManagedShortcut>();
            this.mapToItem = new Dictionary<string, object>();
        }

        public static ShortcutGroup CreateRoot(bool isGlobal = true, bool inherit = false) {
            return new ShortcutGroup(null, null, isGlobal, inherit);
        }

        public string GetPathForName(string name) {
            ValidateName(name);
            return this.FocusGroupPath != null ? (this.FocusGroupPath + '/' + name) : name;
        }

        public ShortcutGroup CreateGroupByName(string name, bool isGlobal = false, bool inherit = false) {
            ValidateName(name, "Group name cannot be null or consist of only whitespaces");
            this.ValidateNameNotInUse(name);
            ShortcutGroup group = new ShortcutGroup(this, name, isGlobal, inherit);
            this.mapToItem[group.FocusGroupName] = group;
            this.groups.Add(group);
            return group;
        }

        public void AddGroup(ShortcutGroup group) {
            ValidateName(group.FocusGroupName, "Group name cannot be null or consist of only whitespaces");
            this.ValidateNameNotInUse(group.FocusGroupName);
            this.mapToItem[group.FocusGroupName] = group;
            this.groups.Add(group);
        }

        public ManagedShortcut AddShortcut(string name, IShortcut shortcut, bool isGlobal = false) {
            ValidateName(name, "Shortcut name cannot be null or consist of only whitespaces");
            this.ValidateNameNotInUse(name);
            ManagedShortcut managed = new ManagedShortcut(this, name, shortcut, isGlobal);
            this.mapToItem[name] = managed;
            this.shortcuts.Add(managed);
            return managed;
        }

        public bool ContainsShortcutByName(string name) {
            return this.mapToItem.TryGetValue(name, out object value) && value is Shortcut;
        }

        public bool ContainsGroupByName(string name) {
            return this.mapToItem.TryGetValue(name, out object value) && value is ShortcutGroup;
        }

        public List<ManagedShortcut> GetShortcutsWithPrimaryStroke(IInputStroke stroke, string focus) {
            List<ManagedShortcut> list = new List<ManagedShortcut>();
            this.CollectShortcutsWithPrimaryStroke(stroke, focus, list);
            return list;
        }

        public void CollectShortcutsWithPrimaryStroke(IInputStroke stroke, string focus, ICollection<ManagedShortcut> list) {
            this.CollectShortcutsInternal(stroke, string.IsNullOrWhiteSpace(focus) ? null : focus, list);
        }

        private void CollectShortcutsInternal(IInputStroke stroke, string focus, ICollection<ManagedShortcut> list) {
            foreach (ShortcutGroup group in this.Groups) {
                group.CollectShortcutsInternal(stroke, focus, list);
            }

            bool requireGlobal = !this.IsGlobal && !this.IsValidSearchForGroup(focus);
            foreach (ManagedShortcut shortcut in this.shortcuts) {
                if (!requireGlobal || shortcut.IsGlobal) {
                    if (shortcut.Shortcut != null && !shortcut.Shortcut.IsEmpty && shortcut.Shortcut.PrimaryStroke.Equals(stroke)) {
                        list.Add(shortcut);
                    }
                }
            }
        }

        private bool IsValidSearchForGroup(string focusedGroup) {
            return IsValidSearchForGroup(this.FocusGroupPath, focusedGroup, this.InheritFromParent);
        }

        private static bool IsValidSearchForGroup(string path, string focused, bool inherit) {
            return path != null && focused != null && (inherit ? focused.StartsWith(path) : focused.Equals(path));
            // return path != null && focused != null && focused.StartsWith(path);
        }

        public ShortcutGroup GetGroupByName(string name) {
            return this.mapToItem.TryGetValue(name, out object value) ? value as ShortcutGroup : null;
        }

        public ShortcutGroup GetGroupByPath(string path) {
            if (string.IsNullOrWhiteSpace(path)) {
                return null;
            }

            int split = path.LastIndexOf('/');
            return split == -1 ? this.GetGroupByName(path) : this.GetGroupByPath(path.Split('/'));
        }

        public ShortcutGroup GetGroupByPath(string[] path) {
            return this.GetGroupByPath(path, 0, path.Length);
        }

        public ShortcutGroup GetGroupByPath(string[] path, int startIndex, int endIndex) {
            if (path == null || (endIndex - startIndex) == 0) {
                return null;
            }

            ValidatePathBounds(path, startIndex, endIndex);
            ShortcutGroup root = this;
            for (int i = startIndex; i < endIndex; i++) {
                if ((root = root.GetGroupByName(path[i])) == null) {
                    return null;
                }
            }

            return root;
        }

        public ManagedShortcut GetShortcutByName(string name) {
            return this.mapToItem.TryGetValue(name, out object value) ? value as ManagedShortcut : null;
        }

        public ManagedShortcut GetShortcutByPath(string path) {
            if (string.IsNullOrWhiteSpace(path)) {
                return null;
            }

            int split = path.LastIndexOf('/');
            return split == -1 ? this.GetShortcutByName(path) : this.GetShortcutByPath(path.Split('/'));
        }

        public ManagedShortcut GetShortcutByPath(string[] path) {
            return this.GetShortcutByPath(path, 0, path.Length);
        }

        public ManagedShortcut GetShortcutByPath(string[] path, int startIndex, int endIndex) {
            if (path == null || (endIndex - startIndex) == 0) {
                return null;
            }

            ValidatePathBounds(path, startIndex, endIndex);
            ShortcutGroup root = this;
            int groupEndIndex = endIndex - 1;
            for (int i = startIndex; i < groupEndIndex; i++) {
                if ((root = root.GetGroupByName(path[i])) == null) {
                    return null;
                }
            }

            return root.GetShortcutByName(path[groupEndIndex]);
        }

        private static void ValidatePathBounds(string[] path, int startIndex, int endIndex) {
            if (startIndex >= path.Length) {
                throw new IndexOutOfRangeException($"startIndex cannot be bigger than or equal to the path length ({startIndex} >= {path.Length})");
            }
            else if (startIndex > endIndex) {
                throw new IndexOutOfRangeException($"startIndex cannot be bigger than endIndex ({startIndex} > {endIndex})");
            }
        }

        private static void ValidateName(string name, string message = "Name cannot be null or consist of only whitespaces") {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException(message, name);
            }
        }

        private void ValidateNameNotInUse(string name) {
            if (this.mapToItem.ContainsKey(name)) {
                string path = this.FocusGroupPath != null ? (this.FocusGroupPath + '/' + name) : name;
                throw new Exception("Group or shortcut already exists with name: " + path);
            }
        }
    }
}