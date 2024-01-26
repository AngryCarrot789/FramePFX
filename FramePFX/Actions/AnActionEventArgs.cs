using System;
using FramePFX.Interactivity;

namespace FramePFX.Actions {
    /// <summary>
    /// Action event arguments for when an action is about to be executed
    /// </summary>
    public class AnActionEventArgs {
        /// <summary>
        /// The action manager associated with this event
        /// </summary>
        public ActionManager Manager { get; }

        /// <summary>
        /// <para>
        /// The data context for this specific action execution. This will not be null, but it may be empty (contain no inner data or data context)
        /// </para>
        /// <para>
        /// In the context of actual context specific actions, this will typically contain the UI control's DataContext,
        /// the data context of the list UI control that it may exist in (e.g. ListBox), and finally the data context for
        /// the shell/window/dialog. There may be more, but these are the main ones that could be available. All of this
        /// gives a wide range of access to the objects being acted upon
        /// </para>
        /// </summary>
        public IDataContext DataContext { get; }

        /// <summary>
        /// Whether this action event was originally caused by a user or not, e.g. via a button/menu click or clicking a check box.
        /// Supply false if this was invoked by, for example, a task or scheduler. A non-user initiated execution usually won't
        /// create error dialogs and may instead log to the console or just throw an exception
        /// <para>
        /// If action events are chain called, it is best to pass the same user initialisation state to the next event
        /// </para>
        /// </summary>
        public bool IsUserInitiated { get; }

        /// <summary>
        /// The action ID associated with this event. Null if the action isn't a fully registered action (and therefore has no ID)
        /// </summary>
        public string ActionId { get; }

        public AnActionEventArgs(ActionManager manager, string actionId, IDataContext dataContext, bool isUserInitiated) {
            if (actionId != null && actionId.Length < 1) {
                throw new ArgumentException("ActionId must be null or a non-empty string");
            }

            if (dataContext == null)
                throw new ArgumentNullException(nameof(dataContext), "Data context cannot be null");

            this.Manager = manager ?? throw new ArgumentNullException(nameof(manager), "Action manager cannot be null");
            this.DataContext = new DataContext(dataContext);
            this.IsUserInitiated = isUserInitiated;
            this.ActionId = actionId;
        }
    }
}