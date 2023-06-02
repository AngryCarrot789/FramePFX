using System;
using System.Collections.Generic;
using FrameControlEx.Core.Shortcuts.Inputs;
using FrameControlEx.Core.Utils;

namespace FrameControlEx.Core.Shortcuts.Managing {
    /// <summary>
    /// A collection of shortcuts
    /// </summary>
    public sealed class ShortcutGroup {
        public const char SeparatorChar = '/';
        public const string SeparatorCharString = "/";

        private readonly List<ShortcutGroup> groups;
        private readonly List<GroupedShortcut> shortcuts;
        private readonly Dictionary<string, object> mapToItem;

        public ShortcutGroup Parent { get; }

        /// <summary>
        /// This group's full path (containing the parent's path and this group's name into one).
        /// It will either be null (meaning no parent), or a non-empty string; it will never consist of only whitespaces
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// This group's name. It will either be null (meaning no parent), or a non-empty string. It will also never consist of only whitespaces
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// This group's display name, which is a more readable and user-friendly version of <see cref="Name"/>
        /// </summary>
        public string DisplayName { get; set; }

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
        public bool Inherit { get; set; }

        /// <summary>
        /// All shortcuts in this focus group
        /// </summary>
        public IEnumerable<GroupedShortcut> Shortcuts => this.shortcuts;

        /// <summary>
        /// All child-groups in this focus group
        /// </summary>
        public IEnumerable<ShortcutGroup> Groups => this.groups;

        public ShortcutGroup(ShortcutGroup parent, string name, bool isGlobal = false, bool inherit = false) {
            this.FullPath = (parent != null && name != null) ? parent.GetPathForName(name) : name;
            this.Name = name;
            this.Inherit = inherit;
            this.IsGlobal = isGlobal;
            this.Parent = parent;
            this.groups = new List<ShortcutGroup>();
            this.shortcuts = new List<GroupedShortcut>();
            this.mapToItem = new Dictionary<string, object>();
        }

        public static ShortcutGroup CreateRoot(bool isGlobal = true, bool inherit = false) {
            return new ShortcutGroup(null, null, isGlobal, inherit);
        }

        public string GetPathForName(string name) {
            ValidateName(name);
            return this.FullPath != null ? StringUtils.Join(this.FullPath, name, SeparatorChar) : name;
        }

        public ShortcutGroup CreateGroupByName(string name, bool isGlobal = false, bool inherit = false) {
            ValidateName(name, "Group name cannot be null or consist of only whitespaces");
            this.ValidateNameNotInUse(name);
            ShortcutGroup group = new ShortcutGroup(this, name, isGlobal, inherit);
            this.mapToItem[group.Name] = group;
            this.groups.Add(group);
            return group;
        }

        public void AddGroup(ShortcutGroup group) {
            ValidateName(group.Name, "Group name cannot be null or consist of only whitespaces");
            this.ValidateNameNotInUse(group.Name);
            this.mapToItem[group.Name] = group;
            this.groups.Add(group);
        }

        public GroupedShortcut AddShortcut(string name, IShortcut shortcut, bool isGlobal = false, bool inherit = false) {
            ValidateName(name, "Shortcut name cannot be null or consist of only whitespaces");
            this.ValidateNameNotInUse(name);
            GroupedShortcut managed = new GroupedShortcut(this, name, shortcut, isGlobal, inherit);
            this.mapToItem[name] = managed;
            this.shortcuts.Add(managed);
            return managed;
        }

        public bool ContainsShortcutByName(string name) {
            return this.mapToItem.TryGetValue(name, out object value) && value is ShortcutGroup;
        }

        public bool ContainsGroupByName(string name) {
            return this.mapToItem.TryGetValue(name, out object value) && value is ShortcutGroup;
        }

        public List<GroupedShortcut> GetShortcutsWithPrimaryStroke(IInputStroke stroke, string focus) {
            List<GroupedShortcut> list = new List<GroupedShortcut>();
            this.CollectShortcutsWithPrimaryStroke(stroke, focus, list);
            return list;
        }

        public void CollectShortcutsWithPrimaryStroke(IInputStroke stroke, string focus, List<GroupedShortcut> list, bool allowDuplicateInheritedShortcuts = false) {
            this.CollectShortcutsInternal(stroke, string.IsNullOrWhiteSpace(focus) ? null : focus, list, allowDuplicateInheritedShortcuts);
        }

        private void CollectShortcutsInternal(IInputStroke stroke, string focus, List<GroupedShortcut> list, bool allowDuplicateInheritedShortcuts = false) {
            // Searching groups first is what allows inheritance to work properly, because you search the deepest
            // levels first and make your way to the root. Similar to how bubble events work
            foreach (ShortcutGroup group in this.Groups) {
                group.CollectShortcutsInternal(stroke, focus, list);
            }

            bool requireGlobal = !this.IsGlobal && !IsFocusPathInScope(this.FullPath, focus, this.Inherit);
            foreach (GroupedShortcut shortcut in this.shortcuts) {
                if (requireGlobal && !shortcut.IsGlobal) {
                    // I actually can't remember if this.FullPath should be used here or shortcut.Path
                    if (shortcut.IsInherited && IsFocusPathInScope(this.FullPath, focus, true)) {
                        if (!allowDuplicateInheritedShortcuts && list.Count > 0) {
                            IInputStroke primary = shortcut.Shortcut.PrimaryStroke; // saves potentially boxing Key/Mouse strokes multiple times
                            if (list.Find(x => x.Shortcut.IsPrimaryStroke(primary)) != null) {
                                continue;
                            }
                        }
                    }
                    else {
                        continue;
                    }
                }

                if (shortcut.Shortcut != null && !shortcut.Shortcut.IsEmpty && shortcut.Shortcut.IsPrimaryStroke(stroke)) {
                    list.Add(shortcut);
                }
            }
        }

        private static bool IsFocusPathInScope(string path, string focused, bool inherit) {
            return path != null && focused != null && (inherit ? focused.StartsWith(path) : focused.Equals(path));
        }

        public GroupedShortcut FindFirstShortcutByAction(string actionId) {
            foreach (GroupedShortcut shortcut in this.shortcuts) {
                if (actionId.Equals(shortcut.ActionId)) {
                    return shortcut;
                }
            }

            foreach (ShortcutGroup group in this.Groups) {
                GroupedShortcut result = group.FindFirstShortcutByAction(actionId);
                if (result != null) {
                    return result;
                }
            }

            return null;
        }

        public ShortcutGroup GetGroupByName(string name) {
            ValidateName(name);
            return this.mapToItem.TryGetValue(name, out object value) ? value as ShortcutGroup : null;
        }

        public ShortcutGroup GetGroupByPath(string path) {
            if (string.IsNullOrWhiteSpace(path)) {
                return null;
            }

            int split = path.LastIndexOf(SeparatorChar);
            return split == -1 ? this.GetGroupByName(path) : this.GetGroupByPath(path.Split(SeparatorChar));
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

        public GroupedShortcut GetShortcutByName(string name) {
            ValidateName(name);
            return this.mapToItem.TryGetValue(name, out object value) ? value as GroupedShortcut : null;
        }

        public GroupedShortcut GetShortcutByPath(string path) {
            if (string.IsNullOrWhiteSpace(path)) {
                return null;
            }

            int split = path.LastIndexOf(SeparatorChar);
            return split == -1 ? this.GetShortcutByName(path) : this.GetShortcutByPath(path.Split(SeparatorChar));
        }

        public GroupedShortcut GetShortcutByPath(string[] path) {
            return this.GetShortcutByPath(path, 0, path.Length);
        }

        public GroupedShortcut GetShortcutByPath(string[] path, int startIndex, int endIndex) {
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
                string path = this.FullPath != null ? StringUtils.Join(this.FullPath, name, SeparatorChar) : name;
                throw new Exception($"Group or shortcut already exists with name: '{path}'");
            }
        }
    }
}