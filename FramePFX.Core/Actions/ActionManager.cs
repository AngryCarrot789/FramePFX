using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Actions {
    public class ActionManager {
        public static ActionManager Instance { get; set; }

        private readonly Dictionary<string, LinkedList<GlobalPresentationUpdateHandler>> updateEventMap;
        private readonly Dictionary<string, AnAction> actions;

        public ActionManager() {
            this.actions = new Dictionary<string, AnAction>();
            this.updateEventMap = new Dictionary<string, LinkedList<GlobalPresentationUpdateHandler>>();
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
            return id != null && this.actions.TryGetValue(id, out AnAction action) ? action : null;
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
                await IoC.MessageDialogs.ShowMessageExAsync("Action execution exception", $"An exception occurred while executing '{e.ActionId ?? action.GetType().ToString()}'", ex.GetToString());
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

        public void AddPresentationUpdateHandler(string id, GlobalPresentationUpdateHandler handler) {
            ValidateId(id);
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");
            }

            if (!this.updateEventMap.TryGetValue(id, out LinkedList<GlobalPresentationUpdateHandler> list)) {
                this.updateEventMap[id] = list = new LinkedList<GlobalPresentationUpdateHandler>();
            }

            list.AddLast(handler);
        }

        public void RemovePresentationUpdateHandler(string id, GlobalPresentationUpdateHandler handler) {
            ValidateId(id);
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");
            }

            if (this.updateEventMap.TryGetValue(id, out var list)) {
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
            if (!this.updateEventMap.TryGetValue(id, out LinkedList<GlobalPresentationUpdateHandler> list))
                return false;
            if (!this.actions.TryGetValue(id, out AnAction action))
                return false;

            AnActionEventArgs args = new AnActionEventArgs(this, id, context, isUserInitiated);
            bool canExecute = action.CanExecute(args);
            foreach (GlobalPresentationUpdateHandler handler in list) {
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