using System;
using SharpPadV2.Core.Actions.Contexts;

namespace SharpPadV2.Core.Actions {
    /// <summary>
    /// Action event arguments for when an action is about to be executed
    /// </summary>
    public class AnActionEventArgs {
        /// <summary>
        /// The action manager associated with this event
        /// </summary>
        public ActionManager Manager { get; }

        /// <summary>
        /// The ID of the action involved with this event
        /// </summary>
        public string ActionId { get; }

        /// <summary>
        /// The data context for this specific action execution. This will not be null, but it may be empty (contain no inner data or data context)
        /// </summary>
        public IDataContext DataContext { get; }

        /// <summary>
        /// Whether this event was originally caused by a user or not
        /// </summary>
        public bool IsUserInitiated { get; }

        public AnActionEventArgs(ActionManager manager, string id, IDataContext dataContext, bool isUserInitiated) {
            this.Manager = manager ?? throw new ArgumentNullException(nameof(manager), "Action manager cannot be null");
            this.DataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext), "Data context cannot be null");
            this.ActionId = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("Action ID cannot be null, empty or whitespaces only", nameof(dataContext)) : id;
            this.IsUserInitiated = isUserInitiated;
        }
    }
}