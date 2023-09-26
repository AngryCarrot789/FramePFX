using System;
using System.Threading.Tasks;
using FramePFX.Actions.Contexts;
using FramePFX.Commands;

namespace FramePFX.Actions {
    /// <summary>
    /// An async command that executes an action
    /// </summary>
    public class ActionCommand : BaseAsyncRelayCommand {
        /// <summary>
        /// The target action ID to execute
        /// </summary>
        public string ActionId { get; set; }

        /// <summary>
        /// Data to pass to the action. Adding data to this is necessary, as actions rely contextual data
        /// </summary>
        public DataContext Context { get; }

        public ActionCommand(string actionId, DataContext context = null) {
            if (string.IsNullOrEmpty(actionId))
                throw new ArgumentException("ActionId cannot be null or empty", nameof(actionId));
            this.ActionId = actionId;
            this.Context = context ?? new DataContext();
        }

        protected override bool CanExecuteCore(object parameter) {
            return ActionManager.Instance.CanExecute(this.ActionId, this.Context);
        }

        protected override Task ExecuteCoreAsync(object parameter) {
            return ActionManager.Instance.Execute(this.ActionId, this.Context);
        }
    }
}