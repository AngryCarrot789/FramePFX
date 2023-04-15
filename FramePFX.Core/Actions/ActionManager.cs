using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FramePFX.Core.Actions.Contexts;

namespace FramePFX.Core.Actions {
    public class ActionManager {
        private readonly Dictionary<string, LinkedList<GlobalPresentationUpdateHandler>> updateEventMap;

        public static ActionManager Instance => CoreIoC.ActionManager;

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
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (TypeInfo typeInfo in assembly.DefinedTypes) {
                    ActionRegistrationAttribute attribute = typeInfo.GetCustomAttribute<ActionRegistrationAttribute>();
                    if (attribute != null) {
                        AnAction action;
                        try {
                            action = (AnAction) Activator.CreateInstance(typeInfo, true);
                        }
                        catch (Exception e) {
                            throw new Exception($"Failed to create an instance of the registered action '{typeInfo.FullName}'", e);
                        }

                        manager.Register(attribute.ActionId, action);
                    }
                }
            }
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
            this.actions[id] = action;
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
            this.actions[id] = new T();
        }

        /// <summary>
        /// Gets an action with the given ID
        /// </summary>
        public virtual AnAction GetAction(string id) {
            return id != null && this.actions.TryGetValue(id, out AnAction action) ? action : null;
        }

        /// <summary>
        /// Tries to execute an action with the given ID and context. The args will be created and errors are handled and passed to <see cref="OnActionException"/>
        /// </summary>
        /// <param name="id">The action ID to execute</param>
        /// <param name="context">The context to use. Cannot be null</param>
        /// <param name="isUserInitiated">Whether a user caused the action to need to be executed. Supply false if this was invoked by a task or scheduler for example</param>
        /// <returns>False if no such action exists, or the action could not execute. Otherwise, true, meaning the action executed successfully</returns>
        /// <exception cref="Exception">The context is null, or the assembly was compiled in debug mode and the action threw ane exception</exception>
        /// <exception cref="ArgumentException">ID is null, empty or consists of only whitespaces</exception>
        /// <exception cref="ArgumentNullException">Context is null</exception>
        public virtual Task<bool> Execute(string id, IDataContext context, bool isUserInitiated = true) {
            ValidateId(id);
            ValidateContext(context);
            if (this.actions.TryGetValue(id, out AnAction action)) {
                AnActionEventArgs args = new AnActionEventArgs(this, id, context, isUserInitiated);
                return action.ExecuteAsync(args); // maybe handle exceptions with an error message if isUserInitiated is false?
            }
            else {
                return this.GetNoSuchActionResult(id, context);
            }
        }

        public virtual Task<bool> GetNoSuchActionResult(string actionId, IDataContext context) {
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
        public virtual Presentation GetPresentation(string id, IDataContext context, bool isUserInitiated = true) {
            ValidateId(id);
            ValidateContext(context);
            if (this.actions.TryGetValue(id, out AnAction action)) {
                AnActionEventArgs args = new AnActionEventArgs(this, id, context, isUserInitiated);
                return action.GetPresentation(args);
            }
            else {
                return this.GetNoSuchActionPresentation(id, context);
            }
        }

        public virtual Presentation GetNoSuchActionPresentation(string actionId, IDataContext context) {
            return Presentation.VisibleAndDisabled;
        }

        public void AddPresentationUpdateEventHandler(string id, GlobalPresentationUpdateHandler handler) {
            ValidateId(id);
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");
            }

            if (!this.updateEventMap.TryGetValue(id, out LinkedList<GlobalPresentationUpdateHandler> list)) {
                this.updateEventMap[id] = list = new LinkedList<GlobalPresentationUpdateHandler>();
            }

            list.AddLast(handler);
        }

        public void RemovePresentationUpdateEventHandler(string id, GlobalPresentationUpdateHandler handler) {
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

            if (this.actions.TryGetValue(id, out AnAction action)) {
                AnActionEventArgs args = new AnActionEventArgs(this, id, context, isUserInitiated);
                Presentation presentation = action.GetPresentation(args);
                foreach (GlobalPresentationUpdateHandler handler in list) {
                    handler(id, action, args, presentation);
                }

                return true;
            }
            else {
                return false;
            }
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