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
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Logger;
using FramePFX.Utils;

namespace FramePFX.CommandSystem {
    public delegate void FocusChangedEventHandler(CommandManager manager, IDataContext newFocus);

    /// <summary>
    /// A class which manages registered commands and the execution of commands
    /// </summary>
    public class CommandManager {
        // using this just in case I soon add more data associated with commands
        private class CommandEntry {
            public readonly Command Command;
            public HashSet<CommandUsageContext> usages;

            public CommandEntry(Command command) {
                this.Command = command;
            }
        }

        public static CommandManager Instance { get; } = new CommandManager();

        private readonly Dictionary<string, CommandEntry> commands;
        private readonly Dictionary<string, HashSet<CommandUsageContext>> commandsWithUsages;
        private readonly HashSet<FocusChangedEventHandler> focusChangeHandlerSet;

        /// <summary>
        /// Gets the number of commands registered
        /// </summary>
        public int Count => this.commands.Count;

        public IEnumerable<KeyValuePair<string, Command>> Commands => this.commands.Select(x => new KeyValuePair<string, Command>(x.Key, x.Value.Command));

        /// <summary>
        /// An event fired when the application's focus changes, possibly invalidating the executability state of a command presentation
        /// </summary>
        public event FocusChangedEventHandler FocusChanged {
            add {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.focusChangeHandlerSet.Add(value);
            }
            remove {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.focusChangeHandlerSet.Remove(value);
            }
        }

        public CommandManager() {
            this.commands = new Dictionary<string, CommandEntry>();
            this.focusChangeHandlerSet = new HashSet<FocusChangedEventHandler>();
            this.commandsWithUsages = new Dictionary<string, HashSet<CommandUsageContext>>();
        }

        public Command Unregister(string id) {
            ValidateId(id);
            if (this.commands.TryGetValue(id, out CommandEntry entry)) {
                this.commands.Remove(id);
                return entry.Command;
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
            if (this.commands.TryGetValue(id, out CommandEntry existing)) {
                throw new Exception($"a command is already registered with the ID '{id}': {existing.Command.GetType()}");
            }

            this.commands[id] = new CommandEntry(command);
        }

        public void RegisterUsage(string cmdId, CommandUsageContext usage) {
            ValidateId(cmdId);
            if (usage == null)
                throw new ArgumentNullException(nameof(usage));
            if (!this.commands.TryGetValue(cmdId, out CommandEntry entry))
                throw new InvalidOperationException("Cannot add usage to non-existent command: " + cmdId);

            HashSet<CommandUsageContext> list1 = entry.usages;
            if (list1 == null)
                entry.usages = list1 = new HashSet<CommandUsageContext>();
            else if (list1.Contains(usage))
                return;
            list1.Add(usage);

            if (!this.commandsWithUsages.TryGetValue(cmdId, out HashSet<CommandUsageContext> list2))
                this.commandsWithUsages[cmdId] = list2 = new HashSet<CommandUsageContext>();
            list2.Add(usage);

            CommandUsageContext.OnRegisteredInternal(usage, cmdId);
        }

        public void UnregisterUsage(string cmdId, CommandUsageContext usage) {
            ValidateId(cmdId);
            if (usage == null)
                throw new ArgumentNullException(nameof(usage));
            if (!this.commands.TryGetValue(cmdId, out CommandEntry entry))
                throw new InvalidOperationException("Cannot remove usage from non-existent command: " + cmdId);

            HashSet<CommandUsageContext> list1 = entry.usages;
            if (list1 != null && list1.Remove(usage)) {
                HashSet<CommandUsageContext> list = this.commandsWithUsages[cmdId];
                if (!list.Remove(usage)) {
                    AppLogger.Instance.WriteLine("Error: failed to remove from commandsWithUsages");
                    Debugger.Break();
                }

                if (list.Count < 1)
                    this.commandsWithUsages.Remove(cmdId);

                CommandUsageContext.OnUnregisteredInternal(usage);
            }
        }

        /// <summary>
        /// Gets a command with the given ID
        /// </summary>
        public virtual Command GetCommandById(string id) {
            return !string.IsNullOrEmpty(id) && this.commands.TryGetValue(id, out CommandEntry command) ? command.Command : null;
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
            if (this.commands.TryGetValue(cmdId, out CommandEntry cmd))
                this.Execute(cmdId, cmd.Command, context, isUserInitiated);
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
            if (!this.commands.TryGetValue(commandId, out CommandEntry command))
                return false;

            IDataContext dataContext = dataContextProvider();
            ValidateContext(dataContext);
            this.ExecuteCore(command.Command, new CommandEventArgs(this, commandId, dataContext, isUserInitiated));
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
            if (!e.IsUserInitiated || Debugger.IsAttached) { // allow debugger to catch exception
                command.Execute(e);
            }
            else {
                TryExecuteOrShowDialog(command, e);
            }
        }

        private static void TryExecuteOrShowDialog(Command command, CommandEventArgs e) {
            try {
                command.Execute(e);
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
            if (!this.commands.TryGetValue(id, out CommandEntry command))
                return false;
            return command.Command.CanExecute(new CommandEventArgs(this, id, context, isUserInitiated));
        }

        /// <summary>
        /// Invokes all focus change handlers for the given ID. This also invokes global handlers first
        /// </summary>
        /// <exception cref="ArgumentNullException">newFocusProvider is null</exception>
        public void OnApplicationFocusChanged(Func<IDataContext> newFocusProvider) {
            if (newFocusProvider == null)
                throw new ArgumentNullException(nameof(newFocusProvider));
            Application.Current.Dispatcher.InvokeAsync(() => {
                this.OnFocusChangeCore(newFocusProvider);
            }, DispatcherPriority.Background);
        }

        private void OnFocusChangeCore(Func<IDataContext> newFocusProvider) {
            // only calls newFocusProvider if there are handlers
            IDataContext ctx = null;
            if (this.focusChangeHandlerSet.Count >= 1) {
                ValidateContext(ctx = newFocusProvider());
                foreach (FocusChangedEventHandler handler in this.focusChangeHandlerSet) {
                    handler(this, ctx);
                }
            }

            if (this.commandsWithUsages.Count > 0) {
                if (ctx == null)
                    ValidateContext(ctx = newFocusProvider());
                // call ToList just in case the handlers registers/unregisters another usage
                foreach (KeyValuePair<string, HashSet<CommandUsageContext>> commandsWithUsage in this.commandsWithUsages.ToList()) {
                    foreach (CommandUsageContext usage in commandsWithUsage.Value) {
                        usage.OnFocusChanged(ctx);
                    }
                }
            }
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

        public void UpdateForFocusChange(CommandUsageContext usage, IDataContext focus) {

        }
    }
}