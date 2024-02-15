using System;
using System.Collections.Generic;
using FramePFX.Shortcuts.Inputs;
using FramePFX.Utils;

namespace FramePFX.Shortcuts.Managing {
    /// <summary>
    /// A collection of shortcuts
    /// </summary>
    public sealed class ShortcutGroup : IGroupedObject {
        public const char SeparatorChar = '/';

        private readonly List<ShortcutGroup> groups;
        private readonly List<GroupedShortcut> shortcuts;
        private readonly List<GroupedInputState> inputStates;
        private readonly Dictionary<string, object> mapToItem;
        private InputStateManager localStateManager;

        public ShortcutManager Manager { get; }

        public ShortcutGroup Parent { get; }

        public string FullPath { get; }

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
        public IReadOnlyList<GroupedShortcut> Shortcuts => this.shortcuts;

        /// <summary>
        /// All input states in this focus group
        /// </summary>
        public IReadOnlyList<GroupedInputState> InputStates => this.inputStates;

        /// <summary>
        /// All child-groups in this focus group
        /// </summary>
        public IReadOnlyList<ShortcutGroup> Groups => this.groups;

        public ShortcutGroup(ShortcutManager manager, ShortcutGroup parent, string name, bool isGlobal = false, bool inherit = false) {
            if (name != null && string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name must be null or a non-empty string that does not consist of only whitespaces");
            this.Manager = manager ?? throw new ArgumentNullException(nameof(manager));
            this.Name = name;
            this.FullPath = parent != null && name != null ? parent.GetPathForName(name) : name;
            this.Inherit = inherit;
            this.IsGlobal = isGlobal;
            this.Parent = parent;
            this.groups = new List<ShortcutGroup>();
            this.shortcuts = new List<GroupedShortcut>();
            this.inputStates = new List<GroupedInputState>();
            this.mapToItem = new Dictionary<string, object>();
        }

        public static ShortcutGroup CreateRoot(ShortcutManager manager, bool isGlobal = true, bool inherit = false) {
            return new ShortcutGroup(manager, null, null, isGlobal, inherit);
        }

        public string GetPathForName(string name) {
            ValidateName(name);
            return this.FullPath != null ? StringUtils.Join(this.FullPath, name, SeparatorChar) : name;
        }

        public ShortcutGroup CreateGroupByName(string name, bool isGlobal = false, bool inherit = false) {
            ValidateName(name, "Group name cannot be null or consist of only whitespaces");
            this.ValidateNameNotInUse(name);
            ShortcutGroup group = new ShortcutGroup(this.Manager, this, name, isGlobal, inherit);
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

        public GroupedShortcut AddShortcut(string name, IShortcut shortcut, bool isGlobal = false) {
            ValidateName(name, "Shortcut name cannot be null or consist of only whitespaces");
            this.ValidateNameNotInUse(name);
            GroupedShortcut managed = new GroupedShortcut(this, name, shortcut, isGlobal);
            this.mapToItem[name] = managed;
            this.shortcuts.Add(managed);
            return managed;
        }

        public GroupedInputState AddInputState(string name, IInputStroke activation, IInputStroke deactivation) {
            ValidateName(name, "Shortcut name cannot be null or consist of only whitespaces");
            this.ValidateNameNotInUse(name);
            GroupedInputState managed = new GroupedInputState(this, name, activation, deactivation);
            this.mapToItem[name] = managed;
            this.inputStates.Add(managed);
            return managed;
        }

        public bool ContainsShortcutByName(string name) {
            return this.mapToItem.TryGetValue(name, out object value) && value is ShortcutGroup;
        }

        public bool ContainsGroupByName(string name) {
            return this.mapToItem.TryGetValue(name, out object value) && value is ShortcutGroup;
        }

        /// <summary>
        /// Old name: CollectShortcutsWithPrimaryStroke
        /// </summary>
        public void EvaulateShortcutsAndInputStates(ref GroupEvaulationArgs args, string focus, bool allowDuplicateInheritedShortcuts = false) {
            this.CollectShortcutsInternal(ref args, string.IsNullOrWhiteSpace(focus) ? null : focus, allowDuplicateInheritedShortcuts);
        }

        private static bool FindPrimaryStroke(List<GroupedShortcut> list, IInputStroke stroke) {
            for (int i = 0, c = list.Count; i < c; i++) {
                if (list[i].Shortcut.IsPrimaryStroke(stroke)) {
                    return true;
                }
            }

            return false;
        }

        private void CollectShortcutsInternal(ref GroupEvaulationArgs args, string focus, bool allowDuplicateInheritedShortcuts = false) {
            // Searching groups first is what allows inheritance to work properly, because you search the deepest
            // levels first and make your way to the root. Similar to how bubble events work
            foreach (ShortcutGroup group in this.groups) {
                group.CollectShortcutsInternal(ref args, focus);
            }

            bool requireGlobal = !this.IsGlobal && !IsFocusPathInScope(this.FullPath, focus, this.Inherit);
            foreach (GroupedShortcut shortcut in this.shortcuts) {
                if (args.filter != null && !args.filter(shortcut)) {
                    continue;
                }

                if (requireGlobal && !shortcut.IsGlobal) {
                    // I actually can't remember if this.FullPath should be used here or shortcut.FullPath
                    if (shortcut.IsInherited && IsFocusPathInScope(this.FullPath, focus, true)) {
                        if (!allowDuplicateInheritedShortcuts && FindPrimaryStroke(args.shortcuts, shortcut.Shortcut.PrimaryStroke)) {
                            continue;
                        }
                    }
                    else {
                        continue;
                    }
                }

                if (!shortcut.Shortcut.IsEmpty && shortcut.Shortcut.IsPrimaryStroke(args.stroke)) {
                    args.shortcuts.Add(shortcut);
                }
            }

            if (args.canProcessInputStates) {
                foreach (GroupedInputState state in this.inputStates) {
                    if (state.ActivationStroke.Equals(args.stroke)) {
                        // IsUsingToggleBehaviour ? !state.IsActive : true
                        args.inputStates.Add((state, !state.IsUsingToggleBehaviour || !state.IsActive));
                    }
                    else if (state.DeactivationStroke.Equals(args.stroke)) {
                        args.inputStates.Add((state, false));
                    }
                }
            }
        }

        private static bool IsFocusPathInScope(string path, string focused, bool inherit) {
            return path != null && focused != null && (inherit ? focused.StartsWith(path) : focused.Equals(path));
        }

        public GroupedShortcut FindFirstShortcutByCommandId(string cmdId) {
            foreach (GroupedShortcut shortcut in this.shortcuts) {
                if (cmdId.Equals(shortcut.CommandId)) {
                    return shortcut;
                }
            }

            foreach (ShortcutGroup group in this.Groups) {
                GroupedShortcut result = group.FindFirstShortcutByCommandId(cmdId);
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

        public override string ToString() {
            return $"{nameof(ShortcutGroup)} ({this.FullPath ?? "<root>"}{(!string.IsNullOrWhiteSpace(this.DisplayName) ? $" \"{this.DisplayName}\"" : "")})";
        }

        public InputStateManager GetInputStateManager() {
            return this.localStateManager ?? (this.localStateManager = this.Manager.GetInputStateManager(this.FullPath));
        }
    }
}