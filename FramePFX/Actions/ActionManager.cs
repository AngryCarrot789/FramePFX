using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FramePFX.Actions.Contexts;
using FramePFX.Utils;

namespace FramePFX.Actions {
    public class ActionManager {
        public static ActionManager Instance { get; set; }

        private readonly Dictionary<string, LinkedList<CanExecuteChangedEventHandler>> updateEventMap;
        private readonly List<CanExecuteChangedEventHandler> globalUpdateEventMap;
        private readonly Dictionary<string, AnAction> actions;

        public int Count => this.actions.Count;

        public IEnumerable<KeyValuePair<string, AnAction>> Actions => this.actions;

        public ActionManager() {
            this.actions = new Dictionary<string, AnAction>();
            this.updateEventMap = new Dictionary<string, LinkedList<CanExecuteChangedEventHandler>>();
            this.globalUpdateEventMap = new List<CanExecuteChangedEventHandler>();
        }

        /// <summary>
        /// Searches all assemblies in the current app domain
        /// </summary>
        /// <exception cref="Exception"></exception>
        public static void SearchAndRegisterActions(ActionManager manager) {
            List<(TypeInfo, ActionRegistrationAttribute)> attributes = new List<(TypeInfo, ActionRegistrationAttribute)>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (TypeInfo typeInfo in assembly.DefinedTypes) {
                    ActionRegistrationAttribute attribute = typeInfo.GetCustomAttribute<ActionRegistrationAttribute>();
                    if (attribute != null) {
                        attributes.Add((typeInfo, attribute));
                    }
                }
            }

            foreach ((TypeInfo type, ActionRegistrationAttribute attribute) in attributes.OrderBy(x => x.Item2.RegistrationOrder)) {
                AnAction action;
                try {
                    action = (AnAction) Activator.CreateInstance(type, true);
                }
                catch (Exception e) {
                    throw new Exception($"Failed to create an instance of the registered action '{type.FullName}'", e);
                }

                if (attribute.OverrideExisting && manager.GetAction(attribute.ActionId) != null) {
                    manager.Unregister(attribute.ActionId);
                }

                manager.Register(attribute.ActionId, action);
            }
        }

        public AnAction Unregister(string id) {
            ValidateId(id);
            if (this.actions.TryGetValue(id, out AnAction action)) {
                this.actions.Remove(id);
                return action;
            }

            return null;
        }

        /// <summary>
        /// Registers an action with the given ID
        /// </summary>
        /// <param name="id">The ID to register the action with</param>
        /// <param name="action">The action to register</param>
        /// <returns>The previous action registered with the given ID</returns>
        /// <exception cref="ArgumentException">Action ID is null or empty</exception>
        /// <exception cref="ArgumentNullException">Action is null</exception>
        public void Register(string id, AnAction action) {
            ValidateId(id);
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            this.RegisterInternal(id, action);
        }

        /// <summary>
        /// Registers an action with the given ID
        /// </summary>
        /// <param name="id">The ID to register the action with</param>
        /// <param name="action">The action to register</param>
        /// <returns>The previous action registered with the given ID</returns>
        /// <exception cref="ArgumentException">Action ID is null or empty</exception>
        /// <exception cref="ArgumentNullException">Action is null</exception>
        public void Register<T>(string id) where T : AnAction, new() {
            ValidateId(id);
            this.RegisterInternal(id, new T());
        }

        private void RegisterInternal(string id, AnAction action) {
            if (this.actions.TryGetValue(id, out AnAction existing)) {
                throw new Exception($"An action is already registered with the ID '{id}': {existing.GetType()}");
            }

            this.actions[id] = action;
        }

        /// <summary>
        /// Gets an action with the given ID
        /// </summary>
        public virtual AnAction GetAction(string id) {
            return !string.IsNullOrEmpty(id) && this.actions.TryGetValue(id, out AnAction action) ? action : null;
        }

        /// <summary>
        /// Executes an action with the given ID and context
        /// </summary>
        /// <param name="id">The action ID to execute</param>
        /// <param name="context">The context to use. Cannot be null</param>
        /// <param name="isUserInitiated">
        /// Whether a user executed the action, e.g. via a button/menu click or clicking a check box.
        /// Supply false if this was invoked by, for example, a task or scheduler. A non-user initiated
        /// execution usually won't create error dialogs and may instead log to the console or just throw an exception
        /// </param>
        /// <returns>False if no such action exists, or the action could not execute. Otherwise, true, meaning the action executed successfully</returns>
        /// <exception cref="Exception">The context is null, or the assembly was compiled in debug mode and the action threw ane exception</exception>
        /// <exception cref="ArgumentException">ID is null, empty or consists of only whitespaces</exception>
        /// <exception cref="ArgumentNullException">Context is null</exception>
        public Task<bool> Execute(string id, IDataContext context, bool isUserInitiated = true) {
            ValidateId(id);
            ValidateContext(context);
            AnActionEventArgs args = new AnActionEventArgs(this, id, context, isUserInitiated);
            if (this.actions.TryGetValue(id, out AnAction action)) {
                return this.ExecuteCore(action, args);
            }
            else {
                return this.GetNoSuchActionResult(args);
            }
        }

        protected virtual Task<bool> ExecuteCore(AnAction action, AnActionEventArgs e) {
            if (e.IsUserInitiated) {
                return Debugger.IsAttached ? action.ExecuteAsync(e) : TryExecuteOrShowDialog(action, e);
            }
            else {
                return action.ExecuteAsync(e);
            }
        }

        private static async Task<bool> TryExecuteOrShowDialog(AnAction action, AnActionEventArgs e) {
            try {
                return await action.ExecuteAsync(e);
            }
            catch (Exception ex) {
                await Services.DialogService.ShowMessageExAsync("Action execution exception", $"An exception occurred while executing '{e.ActionId ?? action.GetType().ToString()}'", ex.GetToString());
                return true;
            }
        }

        public virtual Task<bool> GetNoSuchActionResult(AnActionEventArgs e) {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Tries to get the presentation for an action with the given ID
        /// </summary>
        /// <param name="id">The action ID to execute</param>
        /// <param name="context">The context to use. Cannot be null</param>
        /// <param name="isUserInitiated">Whether a user caused the action to need to be executed. Supply false if this was invoked by a task or scheduler for example</param>
        /// <returns>The action's presentation for the current context</returns>
        /// <exception cref="Exception">The context is null, or the assembly was compiled in debug mode and the GetPresentation function threw ane exception</exception>
        /// <exception cref="ArgumentException">ID is null, empty or consists of only whitespaces</exception>
        /// <exception cref="ArgumentNullException">Context is null</exception>
        public virtual bool CanExecute(string id, IDataContext context, bool isUserInitiated = true) {
            ValidateId(id);
            ValidateContext(context);
            return this.actions.TryGetValue(id, out AnAction action) && action != null && action.CanExecute(new AnActionEventArgs(this, id, context, isUserInitiated));
        }

        public void AddPresentationUpdateHandler(string id, CanExecuteChangedEventHandler handler) {
            if (id != null && string.IsNullOrWhiteSpace(id))
                throw new Exception("ID cannot be empty or whitespaces. It must be null or a valid string");
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");

            if (id == null) {
                if (!this.globalUpdateEventMap.Contains(handler))
                    this.globalUpdateEventMap.Add(handler);
            }
            else {
                if (!this.updateEventMap.TryGetValue(id, out LinkedList<CanExecuteChangedEventHandler> list)) {
                    this.updateEventMap[id] = list = new LinkedList<CanExecuteChangedEventHandler>();
                }

                list.AddLast(handler);
            }
        }

        public void RemovePresentationUpdateHandler(string id, CanExecuteChangedEventHandler handler) {
            if (id != null && string.IsNullOrWhiteSpace(id))
                throw new Exception("ID cannot be empty or whitespaces. It must be null or a valid string");
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");

            if (id == null) {
                this.globalUpdateEventMap.Remove(handler);
            }
            else if (this.updateEventMap.TryGetValue(id, out LinkedList<CanExecuteChangedEventHandler> list)) {
                list.Remove(handler);
            }
        }

        /// <summary>
        /// Invokes all handlers that listen to the given action ID
        /// </summary>
        /// <param name="id">The action ID to execute</param>
        /// <param name="context">The context to use. Cannot be null</param>
        /// <param name="isUserInitiated">Whether a user caused the action to need to be executed. Supply false if this was invoked by a task or scheduler for example</param>
        /// <returns></returns>
        /// <exception cref="Exception">The context is null, or the assembly was compiled in debug mode and the GetPresentation function threw ane exception</exception>
        /// <exception cref="ArgumentException">ID is null, empty or consists of only whitespaces</exception>
        /// <exception cref="ArgumentNullException">Context is null</exception>
        public bool UpdateGlobalPresentation(string id, IDataContext context, bool isUserInitiated = false) {
            ValidateId(id);
            ValidateContext(context);
            if (!this.actions.TryGetValue(id, out AnAction action)) {
                return false;
            }

            if (!this.updateEventMap.TryGetValue(id, out LinkedList<CanExecuteChangedEventHandler> list) && this.globalUpdateEventMap.Count < 1) {
                return false;
            }

            AnActionEventArgs args = new AnActionEventArgs(this, id, context, isUserInitiated);
            bool canExecute = action.CanExecute(args);

            if (list != null) {
                foreach (CanExecuteChangedEventHandler handler in list) {
                    handler(id, action, args, canExecute);
                }
            }

            foreach (CanExecuteChangedEventHandler handler in this.globalUpdateEventMap) {
                handler(id, action, args, canExecute);
            }

            return true;
        }

        public static void ValidateId(string id) {
            if (string.IsNullOrWhiteSpace(id)) {
                throw new ArgumentException("Action ID cannot be null or empty", nameof(id));
            }
        }

        public static void ValidateContext(IDataContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context), "Context cannot be null");
            }
        }
    }
}