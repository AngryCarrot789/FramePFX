using System;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.CommandSystem {
    /// <summary>
    /// Command event arguments for when a command is about to be executed
    /// </summary>
    public class CommandEventArgs {
        /// <summary>
        /// The command manager associated with this event
        /// </summary>
        public CommandManager Manager { get; }

        /// <summary>
        /// The contextual data for this specific command execution. This will not be null, but it may be empty (contain no inner data or data context)
        /// <para>
        /// In terms of actual context-specific commands, this will typically contain the data keys that UI controls
        /// have associated with themselves, merged from top to bottom (top of visual tree to the contextual element).
        /// This gives a wide range of access to the objects being acted upon
        /// </para>
        /// </summary>
        public IDataContext DataContext { get; }

        /// <summary>
        /// Whether this command event was originally caused by a user or not, e.g. via a button/menu click or clicking a check box.
        /// Supply false if this was invoked by, for example, a task or scheduler. A non-user initiated execution usually won't
        /// create error dialogs and may instead log to the console or just throw an exception
        /// <para>
        /// If command events are chain called, it is best to pass the same user initialisation state to the next event
        /// </para>
        /// </summary>
        public bool IsUserInitiated { get; }

        /// <summary>
        /// The command ID associated with this event. Null if the command isn't a fully registered command (and therefore has no ID)
        /// </summary>
        public string CommandId { get; }

        public CommandEventArgs(CommandManager manager, string commandId, IDataContext dataContext, bool isUserInitiated) {
            if (commandId != null && commandId.Length < 1) {
                throw new ArgumentException("CommandId must be null or a non-empty string");
            }

            if (dataContext == null)
                throw new ArgumentNullException(nameof(dataContext), "Data context cannot be null");

            this.Manager = manager ?? throw new ArgumentNullException(nameof(manager), "Command manager cannot be null");
            this.DataContext = new DataContext(dataContext);
            this.IsUserInitiated = isUserInitiated;
            this.CommandId = commandId;
        }
    }
}