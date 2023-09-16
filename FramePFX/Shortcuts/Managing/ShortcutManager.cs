using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Actions.Contexts;
using FramePFX.Shortcuts.Attributes;
using FramePFX.Shortcuts.Events;

namespace FramePFX.Shortcuts.Managing {
    /// <summary>
    /// A class for storing and managing shortcuts
    /// </summary>
    public abstract class ShortcutManager {
        private static readonly Dictionary<Type, CachedTypeData> shortcutPathToMethodCache = new Dictionary<Type, CachedTypeData>();
        private List<GroupedShortcut> allShortcuts;
        private Dictionary<string, LinkedList<GroupedShortcut>> actionToShortcut; // linked because there will only really be like 1 or 2 ever
        private Dictionary<string, GroupedShortcut> pathToShortcut;
        private readonly Dictionary<string, InputStateManager> stateGroups;
        private ShortcutGroup root;

        // event handler storage
        private readonly Dictionary<string, List<ShortcutActivatedEventHandler>> shortcutHandlersMap;
        private readonly List<ShortcutActivatedEventHandler> shortcutHandlersList;
        private readonly List<ShortcutActivatedEventHandler> shortcutHandlersListIgnoreHandlers;

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
        /// monitored (as in, there's no identifiable information except a method handler). Use the action system instead
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
        /// An event that always gets fired whenever a shortcut is activated. These get fired after the path-specific handlers.
        /// <para>
        /// Unlike <see cref="ShortcutActivated"/>, these always get called. However, the bool return value of the handler's task is not used
        /// </para>
        /// <para>
        /// Global shortcut activation handlers are not recommended because their progress cannot be
        /// monitored (as in, there's no identifiable information except a method handler). Use the action system instead
        /// </para>
        /// </summary>
        public event ShortcutActivatedEventHandler MonitorShortcutActivated {
            add {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.shortcutHandlersListIgnoreHandlers.Remove(value); // remove just in case
                this.shortcutHandlersListIgnoreHandlers.Add(value);
            }
            remove {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.shortcutHandlersListIgnoreHandlers.Remove(value);
            }
        }

        /// <summary>
        /// Gets or sets the application wide shortcut manager. Realistically, only 1 needs to exist during the runtime of the app
        /// </summary>
        public static ShortcutManager Instance { get; set; }

        protected ShortcutManager() {
            this.shortcutHandlersMap = new Dictionary<string, List<ShortcutActivatedEventHandler>>();
            this.shortcutHandlersList = new List<ShortcutActivatedEventHandler>();
            this.shortcutHandlersListIgnoreHandlers = new List<ShortcutActivatedEventHandler>();
            this.stateGroups = new Dictionary<string, InputStateManager>();
            this.root = ShortcutGroup.CreateRoot(this);
        }

        /// <summary>
        /// Adds a new shortcut activation handlers, if it isn't already added
        /// <para>
        /// Global shortcut activation handlers are not recommended because their progress cannot be
        /// monitored (as in, there's no identifiable information except a method handler). Use the action system instead
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
        /// monitored (as in, there's no identifiable information except a method handler). Use the action system instead
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
            return this.pathToShortcut.TryGetValue(path, out GroupedShortcut x) ? x : null;
            // return this.Root.GetShortcutByPath(path);
        }

        public GroupedShortcut FindFirstShortcutByAction(string actionId) {
            this.EnsureCacheBuilt();
            return this.actionToShortcut.TryGetValue(actionId, out LinkedList<GroupedShortcut> list) && list.Count > 0 ? list.First.Value : null;
            // return this.Root.FindFirstShortcutByAction(actionId);
        }

        /// <summary>
        /// Called when the root group changes
        /// </summary>
        protected virtual void OnRootChanged(ShortcutGroup oldRoot, ShortcutGroup newRoot) {
        }

        /// <summary>
        /// This will invalidate the cached shortcuts, meaning they will be regenerated when needed
        /// <para>
        /// This should be called if a shortcut or shortcut group was modified (e.g. a new shortcut group and added or a shortcut was removed, shortcut changed)
        /// </para>
        /// </summary>
        public virtual void InvalidateShortcutCache() {
            this.stateGroups.Clear();
            this.allShortcuts = null;
            this.actionToShortcut = null;
            this.pathToShortcut = null;
        }

        /// <summary>
        /// Creates a new shortcut processor for this manager
        /// </summary>
        /// <returns>A new processor</returns>
        public abstract ShortcutInputManager NewProcessor();

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

        protected void EnsureCacheBuilt() {
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

                // path should only be null or non-empty
                if (!string.IsNullOrWhiteSpace(shortcut.FullPath)) {
                    if (this.pathToShortcut.ContainsKey(shortcut.FullPath))
                        throw new Exception("Duplicate shortcut path: " + shortcut.FullPath);
                    this.pathToShortcut[shortcut.FullPath] = shortcut;
                }
            }

            this.allShortcuts.TrimExcess();
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
        /// If none of the event handlers handle the activation, this method calls <see cref="OnShortcutActivatedInternal"/>
        /// which then attempts to invoke the debug reflection handlers, action manager, etc.
        /// </para>
        /// </summary>
        /// <param name="inputManager">The processor that caused this activation</param>
        /// <param name="shortcut">The shortcut that was activated</param>
        /// <returns>A task, which contains the outcome result of the shortcut activation used by the processor's input manager</returns>
        public async Task<bool> OnShortcutActivated(ShortcutInputManager inputManager, GroupedShortcut shortcut) {
            // Fire events first
            IDataContext context = inputManager.CurrentDataContext;
            bool result = false;
            if (this.shortcutHandlersMap.TryGetValue(shortcut.FullPath, out List<ShortcutActivatedEventHandler> list)) {
                foreach (ShortcutActivatedEventHandler handler in list) {
                    if (await handler(inputManager, shortcut, context)) {
                        result = true;
                        break;
                    }
                }
            }

            if (!result) {
                foreach (ShortcutActivatedEventHandler handler in this.shortcutHandlersList) {
                    if (await handler(inputManager, shortcut, context)) {
                        result = true;
                        break;
                    }
                }
            }

            // Fire monitor handlers
            foreach (ShortcutActivatedEventHandler handler in this.shortcutHandlersListIgnoreHandlers) {
                await handler(inputManager, shortcut, context);
            }

            // attempt core activation
            return result || await this.OnShortcutActivatedInternal(inputManager, shortcut);
        }

        /// <summary>
        /// Further attempts to 'activate' a shortcut
        /// </summary>
        /// <param name="inputManager">The processor that caused this activation</param>
        /// <param name="shortcut">The shortcut that was activated</param>
        /// <returns>A task, which contains the outcome result of the shortcut activation used by the processor's input manager</returns>
        protected virtual async Task<bool> OnShortcutActivatedInternal(ShortcutInputManager inputManager, GroupedShortcut shortcut) {
            IDataContext context = inputManager.CurrentDataContext;
            // Fire debug reflection handlers
            foreach (object obj in context.Context) {
                MethodInfo info = CachedTypeData.GetMethod(shortcut.FullPath, obj.GetType(), out int type);
                if (info == null) {
                    continue;
                }

                object ret = info.Invoke(obj, type == 0 ? new object[0] : new object[] {context});
                if (ret is Task task) {
                    if (!task.IsCompleted)
                        await task;
                    return true;
                }
                else {
                    return true;
                }
            }

            // Fire action manager. This is the main way of activating shortcuts
            if (ActionManager.Instance.GetAction(shortcut.ActionId) != null) {
                if (shortcut.ActionContext != null) {
                    DataContext ctx = new DataContext();
                    ctx.Merge(context);
                    ctx.Merge(shortcut.ActionContext);
                    context = ctx;
                }

                IoC.BroadcastShortcutActivity($"Activating shortcut action: {shortcut} -> {shortcut.ActionId}...");
                if (await ActionManager.Instance.Execute(shortcut.ActionId, context)) {
                    IoC.BroadcastShortcutActivity($"Activating shortcut action: {shortcut} -> {shortcut.ActionId}... Complete!");
                    return true;
                }
                else {
                    IoC.BroadcastShortcutActivity($"Activating shortcut action: {shortcut} -> {shortcut.ActionId}... Incomplete!");
                }
            }

            // Nothing handled the shortcut :( Maybe an overridden class will handle them?
            return false;
        }

        /// <summary>
        /// Called by the <see cref="ShortcutInputManager"/> when an input state is activated
        /// </summary>
        /// <param name="inputManager">The processor which caused the state to be activated</param>
        /// <param name="state">The state that was activated</param>
        /// <returns>A task to await</returns>
        protected internal virtual Task OnInputStateActivated(ShortcutInputManager inputManager, GroupedInputState state) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by the <see cref="ShortcutInputManager"/> when an input state is deactivated
        /// </summary>
        /// <param name="inputManager">The processor which caused the state to be deactivated</param>
        /// <param name="state">The state that was activated</param>
        /// <returns>A task to await</returns>
        protected internal virtual Task OnInputStateDeactivated(ShortcutInputManager inputManager, GroupedInputState state) {
            return Task.CompletedTask;
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

        private class CachedTypeData {
            private readonly Type type;
            private readonly Dictionary<string, (MethodInfo, int)> methods;

            private CachedTypeData(Type type, Dictionary<string, (MethodInfo, int)> methods) {
                this.type = type;
                this.methods = methods;
            }

            public static CachedTypeData ForType(Type type) {
                if (shortcutPathToMethodCache.TryGetValue(type, out CachedTypeData data)) {
                    return data;
                }

                List<Type> list = new List<Type>();
                for (Type t = type.BaseType; t != null && !shortcutPathToMethodCache.ContainsKey(t); t = t.BaseType) {
                    list.Add(t);
                }

                for (int i = list.Count - 1; i > 0; i--) {
                    Type t = list[i];
                    shortcutPathToMethodCache[t] = GenerateInfo(t);
                }

                return shortcutPathToMethodCache[type] = GenerateInfo(type);
            }

            public static MethodInfo GetMethod(string path, Type type, out int invocationType) {
                for (Type t = type; t != null; t = t.BaseType) {
                    CachedTypeData data = ForType(t);
                    if (data != null && data.methods.TryGetValue(path, out (MethodInfo, int) info)) {
                        invocationType = info.Item2;
                        return info.Item1;
                    }
                }

                invocationType = 0;
                return null;
            }

            private static CachedTypeData GenerateInfo(Type type) {
                Dictionary<string, (MethodInfo, int)> info = new Dictionary<string, (MethodInfo, int)>();
                foreach (MethodInfo md in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                    ShortcutTargetAttribute attrib = md.GetCustomAttribute<ShortcutTargetAttribute>();
                    if (attrib != null && attrib.ShortcutPaths != null) {
                        int ptype;
                        ParameterInfo[] p = md.GetParameters();
                        if (p.Length == 0) {
                            ptype = 0;
                        }
                        else if (p.Length == 1) {
                            if (p[0].ParameterType == typeof(IDataContext)) {
                                ptype = 1;
                            }
                            else {
                                continue;
                            }
                        }
                        else {
                            continue;
                        }

                        foreach (string path in attrib.ShortcutPaths) {
                            info[path] = (md, ptype);
                        }
                    }
                }

                return info.Count > 0 ? new CachedTypeData(type, info) : null;
            }
        }
    }
}