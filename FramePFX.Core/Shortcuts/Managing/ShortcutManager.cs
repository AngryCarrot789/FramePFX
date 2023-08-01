using System;
using System.Collections.Generic;
using FramePFX.Core.Shortcuts.Inputs;

namespace FramePFX.Core.Shortcuts.Managing {
    /// <summary>
    /// A class for storing and managing shortcuts
    /// </summary>
    public abstract class ShortcutManager {
        public static ShortcutManager Instance { get; set; }

        private List<GroupedShortcut> allShortcuts;
        private Dictionary<string, LinkedList<GroupedShortcut>> actionToShortcut; // linked because there will only really be like 1 or 2 ever
        private Dictionary<string, GroupedShortcut> pathToShortcut;

        private ShortcutGroup root;
        public ShortcutGroup Root {
            get => this.root;
            protected set {
                ShortcutGroup old = this.root;
                this.root = value;
                this.OnRootChanged(old, value);
            }
        }

        public ShortcutManager() {
            this.Root = ShortcutGroup.CreateRoot();
        }

        public ShortcutGroup FindGroupByPath(string path) {
            return this.Root.GetGroupByPath(path);
        }

        public GroupedShortcut FindShortcutByPath(string path) {
            this.EnsureCacheBuilt();
            return this.pathToShortcut.TryGetValue(path, out GroupedShortcut x) ? x : null;
            // return this.Root.GetShortcutByPath(path);
        }

        public GroupedShortcut FindFirstShortcutByAction(string actionId) {
            this.EnsureCacheBuilt();
            return this.actionToShortcut.TryGetValue(actionId, out LinkedList<GroupedShortcut> list) && list.Count > 0 ? list.First.Value : null;
            // return this.Root.FindFirstShortcutByAction(actionId);
        }

        protected virtual void OnRootChanged(ShortcutGroup oldRoot, ShortcutGroup newRoot) {
            this.InvalidateShortcutCache();
        }

        /// <summary>
        /// This will invalidate the cached shortcuts, meaning they will be regenerated when needed
        /// <para>
        /// This should be called if a shortcut or shortcut group was modified (e.g. a new shortcut group and added or a shortcut was removed, shortcut changed)
        /// </para>
        /// </summary>
        public virtual void InvalidateShortcutCache() {
            this.allShortcuts = null;
            this.actionToShortcut = null;
            this.pathToShortcut = null;
        }

        /// <summary>
        /// Creates a new shortcut processor for this manager
        /// </summary>
        /// <returns></returns>
        public abstract ShortcutProcessor NewProcessor();

        public IEnumerable<GroupedShortcut> GetAllShortcuts() {
            this.EnsureCacheBuilt();
            return this.allShortcuts;
        }

        public IEnumerable<GroupedShortcut> GetShortcutsByAction(string actionId) {
            this.EnsureCacheBuilt();
            return this.actionToShortcut.TryGetValue(actionId, out LinkedList<GroupedShortcut> value) ? value : null;
        }

        public static void GetAllShortcuts(ShortcutGroup rootGroup, ICollection<GroupedShortcut> accumulator) {
            foreach (GroupedShortcut shortcut in rootGroup.Shortcuts) {
                accumulator.Add(shortcut);
            }

            foreach (ShortcutGroup innerGroup in rootGroup.Groups) {
                GetAllShortcuts(innerGroup, accumulator);
            }
        }

        private void EnsureCacheBuilt() {
            if (this.allShortcuts == null) {
                this.RebuildShortcutCache();
            }
        }

        private void RebuildShortcutCache() {
            this.actionToShortcut = new Dictionary<string, LinkedList<GroupedShortcut>>();
            this.pathToShortcut = new Dictionary<string, GroupedShortcut>();
            this.allShortcuts = new List<GroupedShortcut>(64);
            if (this.root != null) {
                GetAllShortcuts(this.root, this.allShortcuts);
            }

            foreach (GroupedShortcut shortcut in this.allShortcuts) {
                if (!string.IsNullOrWhiteSpace(shortcut.ActionId)) {
                    if (!this.actionToShortcut.TryGetValue(shortcut.ActionId, out LinkedList<GroupedShortcut> list)) {
                        this.actionToShortcut[shortcut.ActionId] = list = new LinkedList<GroupedShortcut>();
                    }

                    list.AddLast(shortcut);
                }

                if (!string.IsNullOrWhiteSpace(shortcut.FullPath)) { // should only be null or non-empty
                    this.pathToShortcut[shortcut.FullPath] = shortcut;
                }
            }
        }

        public virtual void CollectShortcutsWithPrimaryStroke(IInputStroke stroke, string focus, List<GroupedShortcut> shortcuts, Predicate<GroupedShortcut> filter = null) {
            ShortcutCollectorArgs args = new ShortcutCollectorArgs(stroke, shortcuts, filter);
            this.root.CollectShortcutsWithPrimaryStroke(ref args, focus);
        }

        public IEnumerable<GroupedShortcut> FindShortcutsByPaths(IEnumerable<string> paths) {
            foreach (string path in paths) {
                GroupedShortcut shortcut = this.FindShortcutByPath(path);
                if (shortcut != null) {
                    yield return shortcut;
                }
            }
        }
    }
}