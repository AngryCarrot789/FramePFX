//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;
using FramePFX.Shortcuts.Events;
using FramePFX.Shortcuts.Inputs;
using FramePFX.Shortcuts.Usage;

namespace FramePFX.Shortcuts.Managing {
    public delegate void ShortcutActivityEventHandler(ShortcutInputManager manager);

    /// <summary>
    /// A class for storing and managing shortcuts
    /// </summary>
    public abstract class ShortcutManager {
        private List<GroupedShortcut> cachedAllShortcuts;
        private readonly Dictionary<string, LinkedList<GroupedShortcut>> cachedCmdToShortcut; // linked because there will only really be like 1 or 2 ever
        private readonly Dictionary<string, GroupedShortcut> cachedPathToShortcut;
        private readonly Dictionary<string, InputStateManager> stateGroups;
        private ShortcutGroup root;

        // event handler storage
        private readonly Dictionary<string, List<ShortcutActivatedEventHandler>> shortcutHandlersMap;
        private readonly List<ShortcutActivatedEventHandler> shortcutHandlersList;

        public ShortcutGroup Root {
            get => this.root;
            protected set {
                ShortcutGroup old = this.root;
                this.root = value;
                this.OnRootChanged(old, value);
            }
        }

        /// <summary>
        /// An event that gets fired whenever a shortcut is activated. Some or even none of the handlers may get invoked
        /// if a previous handler was already handled. These get fired after the path-specific handlers
        /// <para>
        /// Global shortcut activation handlers are not recommended because their progress cannot be
        /// monitored (as in, there's no identifiable information except a method handler). Use the command system instead
        /// </para>
        /// </summary>
        public event ShortcutActivatedEventHandler ShortcutActivated {
            add {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.shortcutHandlersList.Remove(value); // remove just in case
                this.shortcutHandlersList.Add(value);
            }
            remove {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.shortcutHandlersList.Remove(value);
            }
        }

        /// <summary>
        /// An event fired when a <see cref="GroupedShortcut"/>'s shortcut is modified
        /// </summary>
        public event ShortcutModifiedEventHandler<GroupedShortcut> ShortcutModified;

        /// <summary>
        /// Gets or sets the application wide shortcut manager. Realistically, only 1 needs to exist during the runtime of the app
        /// </summary>
        public static ShortcutManager Instance { get; set; }

        protected ShortcutManager() {
            this.cachedCmdToShortcut = new Dictionary<string, LinkedList<GroupedShortcut>>();
            this.cachedPathToShortcut = new Dictionary<string, GroupedShortcut>();
            this.stateGroups = new Dictionary<string, InputStateManager>();
            this.shortcutHandlersMap = new Dictionary<string, List<ShortcutActivatedEventHandler>>();
            this.shortcutHandlersList = new List<ShortcutActivatedEventHandler>();
            this.root = ShortcutGroup.CreateRoot(this);
        }

        /// <summary>
        /// Adds a new shortcut activation handlers, if it isn't already added
        /// <para>
        /// Global shortcut activation handlers are not recommended because their progress cannot be
        /// monitored (as in, there's no identifiable information except a method handler). Use the command system instead
        /// </para>
        /// </summary>
        /// <param name="path">The full shortcut path (e.g. App/MyGroup/CoolShortcut)</param>
        /// <returns>True if the handler was added, false if it was moved to the end of the list</returns>
        public bool AddShortcutActivationHandler(string path, ShortcutActivatedEventHandler handler) {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            int i = -1;
            if (!this.shortcutHandlersMap.TryGetValue(path, out List<ShortcutActivatedEventHandler> list)) {
                this.shortcutHandlersMap[path] = list = new List<ShortcutActivatedEventHandler>();
            }
            else if ((i = list.IndexOf(handler)) != -1) {
                list.RemoveAt(i);
            }

            list.Add(handler);
            return i == -1;
        }

        /// <summary>
        /// Removes a shortcut handler for the given path, if it was added (duh)
        /// <para>
        /// Global shortcut activation handlers are not recommended because their progress cannot be
        /// monitored (as in, there's no identifiable information except a method handler). Use the command system instead
        /// </summary>
        /// <param name="path">The full shortcut path (e.g. App/MyGroup/CoolShortcut)</param>
        /// <param name="handler">The handler to remove</param>
        /// <returns>True if the handler was removed, otherwise false</returns>
        public bool RemoveShortcutActivationHandler(string path, ShortcutActivatedEventHandler handler) {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            return this.shortcutHandlersMap.TryGetValue(path, out List<ShortcutActivatedEventHandler> list) && list.Remove(handler);
        }

        public ShortcutGroup FindGroupByPath(string path) {
            return this.Root.GetGroupByPath(path);
        }

        public GroupedShortcut FindShortcutByPath(string path) {
            this.EnsureCacheBuilt();
            return this.cachedPathToShortcut.TryGetValue(path, out GroupedShortcut x) ? x : null;
            // return this.Root.GetShortcutByPath(path);
        }

        public GroupedShortcut FindFirstShortcutByCommandId(string cmdId) {
            this.EnsureCacheBuilt();
            return this.cachedCmdToShortcut.TryGetValue(cmdId, out LinkedList<GroupedShortcut> list) && list.Count > 0 ? list.First.Value : null;
        }

        /// <summary>
        /// Called when the root group changes
        /// </summary>
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
            this.cachedAllShortcuts = null;
            this.stateGroups.Clear();
            this.cachedCmdToShortcut.Clear();
            this.cachedPathToShortcut.Clear();
        }

        /// <summary>
        /// Creates a new shortcut processor for this manager
        /// </summary>
        /// <returns>A new processor</returns>
        public abstract ShortcutInputManager NewProcessor();

        public IEnumerable<GroupedShortcut> GetAllShortcuts() {
            this.EnsureCacheBuilt();
            return this.cachedAllShortcuts;
        }

        public IEnumerable<GroupedShortcut> GetShortcutsByCommandId(string cmdId) {
            this.EnsureCacheBuilt();
            return this.cachedCmdToShortcut.TryGetValue(cmdId, out LinkedList<GroupedShortcut> value) ? value : null;
        }

        public static void GetAllShortcuts(ShortcutGroup rootGroup, ICollection<GroupedShortcut> accumulator) {
            foreach (GroupedShortcut shortcut in rootGroup.Shortcuts) {
                accumulator.Add(shortcut);
            }

            foreach (ShortcutGroup innerGroup in rootGroup.Groups) {
                GetAllShortcuts(innerGroup, accumulator);
            }
        }

        protected void EnsureCacheBuilt() {
            if (this.cachedAllShortcuts == null) {
                this.RebuildShortcutCache();
            }
        }

        private void RebuildShortcutCache() {
            this.cachedAllShortcuts = new List<GroupedShortcut>(64);
            this.cachedCmdToShortcut.Clear();
            this.cachedPathToShortcut.Clear();
            if (this.root != null) {
                GetAllShortcuts(this.root, this.cachedAllShortcuts);
            }

            foreach (GroupedShortcut shortcut in this.cachedAllShortcuts) {
                if (!string.IsNullOrWhiteSpace(shortcut.CommandId)) {
                    if (!this.cachedCmdToShortcut.TryGetValue(shortcut.CommandId, out LinkedList<GroupedShortcut> list)) {
                        this.cachedCmdToShortcut[shortcut.CommandId] = list = new LinkedList<GroupedShortcut>();
                    }

                    list.AddLast(shortcut);
                }

                // path should only be null or non-empty
                if (!string.IsNullOrWhiteSpace(shortcut.FullPath)) {
                    if (this.cachedPathToShortcut.ContainsKey(shortcut.FullPath))
                        throw new Exception("Duplicate shortcut path: " + shortcut.FullPath);
                    this.cachedPathToShortcut[shortcut.FullPath] = shortcut;
                }
            }

            this.cachedAllShortcuts.TrimExcess();
        }

        public IEnumerable<GroupedShortcut> FindShortcutsByPaths(IEnumerable<string> paths) {
            foreach (string path in paths) {
                GroupedShortcut shortcut = this.FindShortcutByPath(path);
                if (shortcut != null) {
                    yield return shortcut;
                }
            }
        }

        /// <summary>
        /// Invoked when a <see cref="ShortcutInputManager"/> activates a shortcut. This should be called first, in order to
        /// fire the <see cref="MonitorShortcutActivated"/> event handlers.
        /// <para>
        /// If none of the event handlers handle the activation, this method calls <see cref="OnShortcutActivatedOverride"/>
        /// which then attempts to invoke the debug reflection handlers, command manager, etc.
        /// </para>
        /// </summary>
        /// <param name="inputManager">The processor that caused this activation</param>
        /// <param name="shortcut">The shortcut that was activated</param>
        /// <returns>The outcome of the shortcut activation used by the processor's input manager</returns>
        public async Task<bool> OnShortcutActivated(ShortcutInputManager inputManager, GroupedShortcut shortcut) {
            // Fire events first
            bool result = false;
            IContextData context = null;
            if (this.shortcutHandlersMap.TryGetValue(shortcut.FullPath, out List<ShortcutActivatedEventHandler> list)) {
                foreach (ShortcutActivatedEventHandler handler in list) {
                    result |= handler(inputManager, shortcut, context ?? (context = inputManager.GetCurrentContext()));
                }
            }

            foreach (ShortcutActivatedEventHandler handler in this.shortcutHandlersList) {
                result |= handler(inputManager, shortcut, context ?? (context = inputManager.GetCurrentContext()));
            }

            // this.OnShortcutActivatedOverride is called here due to | not ||
            return result | await this.OnShortcutActivatedOverride(inputManager, shortcut);
        }

        /// <summary>
        /// Further attempts to 'activate' a shortcut
        /// </summary>
        /// <param name="inputManager">The processor that caused this activation</param>
        /// <param name="shortcut">The shortcut that was activated</param>
        /// <returns>The result of the shortcut activation used by the processor's input manager</returns>
        protected virtual async Task<bool> OnShortcutActivatedOverride(ShortcutInputManager inputManager, GroupedShortcut shortcut) {
            // Fire command. This is the main way of activating shortcuts
            return await CommandManager.Instance.TryExecute(shortcut.CommandId, inputManager.ProvideCurrentContextInternal);
        }

        /// <summary>
        /// Called by the <see cref="ShortcutInputManager"/> when an input state is activated
        /// </summary>
        /// <param name="inputManager">The processor which caused the state to be activated</param>
        /// <param name="state">The state that was activated</param>
        protected internal virtual void OnInputStateActivated(ShortcutInputManager inputManager, GroupedInputState state) {
        }

        /// <summary>
        /// Called by the <see cref="ShortcutInputManager"/> when an input state is deactivated
        /// </summary>
        /// <param name="inputManager">The processor which caused the state to be deactivated</param>
        /// <param name="state">The state that was activated</param>
        protected internal virtual void OnInputStateDeactivated(ShortcutInputManager inputManager, GroupedInputState state) {
        }

        /// <summary>
        /// Gets or creates an <see cref="InputStateManager"/> for the given path
        /// </summary>
        /// <param name="id">The state manager's ID, which is shared across this <see cref="ShortcutManager"/> instance</param>
        /// <returns>An existing or new instance</returns>
        public InputStateManager GetInputStateManager(string id) {
            if (!this.stateGroups.TryGetValue(id, out InputStateManager group))
                this.stateGroups[id] = group = new InputStateManager(this, id);
            return group;
        }

        public void OnShortcutModified(GroupedShortcut shortcut, IShortcut oldShortcut) {
            this.InvalidateShortcutCache();
            this.ShortcutModified?.Invoke(shortcut, oldShortcut);
        }

        protected internal virtual void OnSecondShortcutUsagesProgressed(ShortcutInputManager inputManager) {
        }

        protected internal virtual void OnShortcutUsagesCreated(ShortcutInputManager inputManager) {
        }

        protected internal virtual void OnCancelUsageForNoSuchNextMouseStroke(ShortcutInputManager inputManager, IShortcutUsage usage, GroupedShortcut shortcut, MouseStroke stroke) {
        }

        protected internal virtual void OnCancelUsageForNoSuchNextKeyStroke(ShortcutInputManager inputManager, IShortcutUsage usage, GroupedShortcut shortcut, KeyStroke stroke) {
        }

        protected internal virtual void OnNoSuchShortcutForMouseStroke(ShortcutInputManager inputManager, string group, MouseStroke stroke) {
        }

        protected internal virtual void OnNoSuchShortcutForKeyStroke(ShortcutInputManager inputManager, string group, KeyStroke stroke) {
        }
    }
}