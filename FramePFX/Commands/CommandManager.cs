using System;
using System.Collections.Generic;
using System.Diagnostics;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Utils;

namespace FramePFX.Commands {
    /// <summary>
    /// A class which manages registered commands and the execution of commands
    /// </summary>
    public class CommandManager {
        public static CommandManager Instance { get; } = new CommandManager();

        private readonly Dictionary<string, LinkedList<CanExecuteChangedEventHandler>> updateEventMap;
        private readonly List<CanExecuteChangedEventHandler> globalUpdateEventMap;
        private readonly Dictionary<string, Command> commands;

        /// <summary>
        /// Gets the number of commands registered
        /// </summary>
        public int Count => this.commands.Count;

        public IEnumerable<KeyValuePair<string, Command>> Commands => this.commands;

        public CommandManager() {
            this.commands = new Dictionary<string, Command>();
            this.updateEventMap = new Dictionary<string, LinkedList<CanExecuteChangedEventHandler>>();
            this.globalUpdateEventMap = new List<CanExecuteChangedEventHandler>();
        }

        public Command Unregister(string id) {
            ValidateId(id);
            if (this.commands.TryGetValue(id, out Command cmd)) {
                this.commands.Remove(id);
                return cmd;
            }

            return null;
        }

        /// <summary>
        /// Registers a command with the given ID
        /// </summary>
        /// <param name="id">The ID to register the command with</param>
        /// <param name="command">The command to register</param>
        /// <exception cref="ArgumentException">Command ID is null or empty</exception>
        /// <exception cref="ArgumentNullException">Command is null</exception>
        public void Register(string id, Command command) {
            ValidateId(id);
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            this.RegisterInternal(id, command);
        }

        private void RegisterInternal(string id, Command command) {
            if (this.commands.TryGetValue(id, out Command existing)) {
                throw new Exception($"a command is already registered with the ID '{id}': {existing.GetType()}");
            }

            this.commands[id] = command;
        }

        /// <summary>
        /// Gets a command with the given ID
        /// </summary>
        public virtual Command GetCommandById(string id) {
            return !string.IsNullOrEmpty(id) && this.commands.TryGetValue(id, out Command command) ? command : null;
        }

        /// <summary>
        /// Executes a command with the given ID and context
        /// </summary>
        /// <param name="cmdId">The command ID to execute</param>
        /// <param name="context">The context to use. Cannot be null</param>
        /// <param name="isUserInitiated">
        /// Whether a user executed the command, e.g. via a button/menu click or clicking a check box.
        /// Supply false if this was invoked by, for example, a task or scheduler. A non-user initiated
        /// execution usually won't create error dialogs and may instead log to the console or just throw an exception
        /// </param>
        /// <exception cref="Exception">The context is null, or the assembly was compiled in debug mode and the command threw ane exception</exception>
        /// <exception cref="ArgumentException">ID is null, empty or consists of only whitespaces</exception>
        /// <exception cref="ArgumentNullException">Context is null</exception>
        public void Execute(string cmdId, IDataContext context, bool isUserInitiated = true) {
            ValidateId(cmdId);
            if (this.commands.TryGetValue(cmdId, out Command cmd))
                this.Execute(cmdId, cmd, context, isUserInitiated);
        }

        /// <summary>
        /// Tries to execute the given command. If it does not exist, this method returns a task with a result
        /// of false. Otherwise, a result with a result of true. Command completion is too subjective which is why
        /// this returns the command existence boolean
        /// </summary>
        /// <param name="commandId">The target command id</param>
        /// <param name="dataContextProvider">A function that provides the data context if required (if the command exists)</param>
        /// <param name="isUserInitiated">True when executed as a user, which is usually the default</param>
        public bool TryExecute(string commandId, Func<IDataContext> dataContextProvider, bool isUserInitiated = true) {
            ValidateId(commandId);
            if (dataContextProvider == null)
                throw new ArgumentNullException(nameof(dataContextProvider), "Data context provider cannot be null");
            if (!this.commands.TryGetValue(commandId, out Command command))
                return false;

            IDataContext dataContext = dataContextProvider();
            ValidateContext(dataContext);
            this.ExecuteCore(command, new CommandEventArgs(this, commandId, dataContext, isUserInitiated));
            return true;
        }

        /// <summary>
        /// Executes the command with the given (optional) command ID
        /// </summary>
        /// <param name="cmdId">The target command id</param>
        /// <param name="cmd"></param>
        /// <param name="context"></param>
        /// <param name="isUserInitiated"></param>
        public void Execute(string cmdId, Command cmd, IDataContext context, bool isUserInitiated = true) {
            ValidateId(cmdId);
            ValidateContext(context);
            this.ExecuteCore(cmd, new CommandEventArgs(this, cmdId, context, isUserInitiated));
        }

        protected virtual void ExecuteCore(Command command, CommandEventArgs e) {
            if (e.IsUserInitiated) {
                if (Debugger.IsAttached) {
                    command.ExecuteAsync(e);
                }
                else {
                    TryExecuteOrShowDialog(command, e);
                }
            }
            else {
                command.ExecuteAsync(e);
            }
        }

        private static void TryExecuteOrShowDialog(Command command, CommandEventArgs e) {
            try {
                command.ExecuteAsync(e);
            }
            catch (Exception ex) {
                IoC.MessageService.ShowMessage("Command execution exception", $"An exception occurred while executing '{e.CommandId ?? command.GetType().ToString()}'", ex.GetToString());
            }
        }

        /// <summary>
        /// Tries to get the presentation for a command with the given ID
        /// </summary>
        /// <param name="id">The command ID to execute</param>
        /// <param name="context">The context to use. Cannot be null</param>
        /// <param name="isUserInitiated">Whether a user caused the command to need to be executed. Supply false if this was invoked by a task or scheduler for example</param>
        /// <returns>The command's presentation for the current context</returns>
        /// <exception cref="Exception">The context is null, or the assembly was compiled in debug mode and the GetPresentation function threw ane exception</exception>
        /// <exception cref="ArgumentException">ID is null, empty or consists of only whitespaces</exception>
        /// <exception cref="ArgumentNullException">Context is null</exception>
        public virtual bool CanExecute(string id, IDataContext context, bool isUserInitiated = true) {
            ValidateId(id);
            ValidateContext(context);
            if (!this.commands.TryGetValue(id, out Command command))
                return false;
            return command.CanExecute(new CommandEventArgs(this, id, context, isUserInitiated));
        }

        public void AddCanUpdateHandler(string id, CanExecuteChangedEventHandler handler) {
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

        public void RemoveCanUpdateHandler(string id, CanExecuteChangedEventHandler handler) {
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
        /// Invokes all handlers that listen to the given command ID
        /// </summary>
        /// <param name="id">The command ID to execute</param>
        /// <param name="context">The context to use. Cannot be null</param>
        /// <param name="isUserInitiated">Whether a user caused the command to need to be executed. Supply false if this was invoked by a task or scheduler for example</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">ID is null, empty or consists of only whitespaces</exception>
        /// <exception cref="ArgumentNullException">Context is null</exception>
        public bool OnCanUpdateChanged(string id, IDataContext context, bool isUserInitiated = false) {
            ValidateId(id);
            ValidateContext(context);
            if (!this.commands.TryGetValue(id, out Command command)) {
                return false;
            }

            if (!this.updateEventMap.TryGetValue(id, out LinkedList<CanExecuteChangedEventHandler> list) && this.globalUpdateEventMap.Count < 1) {
                return false;
            }

            CommandEventArgs args = new CommandEventArgs(this, id, context, isUserInitiated);
            bool canExecute = command.CanExecute(args);

            if (list != null) {
                foreach (CanExecuteChangedEventHandler handler in list) {
                    handler(id, command, args, canExecute);
                }
            }

            foreach (CanExecuteChangedEventHandler handler in this.globalUpdateEventMap) {
                handler(id, command, args, canExecute);
            }

            return true;
        }

        public static void ValidateId(string id) {
            if (string.IsNullOrWhiteSpace(id)) {
                throw new ArgumentException("Command ID cannot be null or empty", nameof(id));
            }
        }

        public static void ValidateContext(IDataContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context), "Context cannot be null");
            }
        }
    }
}