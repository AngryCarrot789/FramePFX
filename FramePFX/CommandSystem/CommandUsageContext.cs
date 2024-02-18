using FramePFX.Interactivity.DataContexts;

namespace FramePFX.CommandSystem {
    /// <summary>
    /// A class that represents the state of a single instance of a command being used. This manages its own executability state and handles application focus change
    /// </summary>
    public abstract class CommandUsageContext {
        /// <summary>
        /// Gets the command that this usage is linked to
        /// </summary>
        public Command Command => CommandManager.Instance.GetCommandById(this.CommandId);

        public string CommandId { get; private set; }

        protected CommandUsageContext() {
        }

        protected virtual void OnAttached() {
        }

        protected virtual void OnDetatched() {
        }

        /// <summary>
        /// Called when the application's focus changes
        /// </summary>
        public virtual void OnFocusChanged(IDataContext newFocus) {
            CommandManager.Instance.UpdateForFocusChange(this, newFocus);
        }

        /// <summary>
        /// Called when the executability state of this usage's command may have changed
        /// </summary>
        /// <param name="context">The context to use to evaluate the executability</param>
        public virtual void OnCanExecuteInvalidated(IDataContext context) {

        }

        internal static void OnRegisteredInternal(CommandUsageContext usage, string cmdId) {
            usage.CommandId = cmdId;
            usage.OnAttached();
        }

        internal static void OnUnregisteredInternal(CommandUsageContext usage) {
            if (usage.CommandId == null)
                return;
            try {
                usage.OnDetatched();
            }
            finally {
                usage.CommandId = null;
            }
        }
    }
}