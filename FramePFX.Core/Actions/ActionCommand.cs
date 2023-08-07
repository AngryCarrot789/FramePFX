using System;
using System.Threading.Tasks;
using FramePFX.Core.Actions.Contexts;

namespace FramePFX.Core.Actions
{
    /// <summary>
    /// An async command that executes an action
    /// </summary>
    public class ActionCommand : BaseAsyncRelayCommand
    {
        /// <summary>
        /// The target action ID to execute
        /// </summary>
        public string ActionId { get; set; }

        /// <summary>
        /// Additional data context to
        /// </summary>
        public DataContext Context { get; }

        public ActionCommand(string actionId, DataContext context = null)
        {
            if (string.IsNullOrEmpty(actionId))
                throw new ArgumentException("ActionId cannot be null or empty", nameof(actionId));
            this.ActionId = actionId;
            this.Context = context ?? new DataContext();
        }

        public AnAction GetAction(ActionManager manager)
        {
            return manager.GetAction(this.ActionId);
        }

        protected override bool CanExecuteCore(object parameter)
        {
            return ActionManager.Instance.CanExecute(this.ActionId, this.Context);
        }

        protected override Task ExecuteCoreAsync(object parameter)
        {
            return ActionManager.Instance.Execute(this.ActionId, this.Context);
        }
    }
}