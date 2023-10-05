using System.Threading.Tasks;

namespace FramePFX.Actions {
    /// <summary>
    /// Represents some sort of action that can be executed. This class is designed to be used as a singleton,
    /// meaning there should only ever be a single instance of any implementation of this class
    /// <para>
    /// These actions can be executed through the <see cref="ActionManager.Execute(string, Contexts.IDataContext, bool)"/> function
    /// </para>
    /// </summary>
    public abstract class AnAction {
        protected AnAction() {
        }

        /// <summary>
        /// <para>
        /// Executes this specific action with the given action event args
        /// </para>
        /// <para>
        /// About the return value: When executed by a shortcut processor, the return value is used along side
        /// the final outcome of the processor input event. Typically, the first action to return true is the
        /// last action to be invoked in that specific frame and causes the input event to be handled.
        /// </para>
        /// <para>
        /// In this case, it's typically a better option for the return value to be whether this action is actually executable
        /// in some form, instead of whether if it executed successfully or not
        /// </para>
        /// <para>
        /// For example, RemoveSelectedItemsAction: the args data context did not contain some sort of "list" object, so returning false
        /// makes sense. However, if it did contain a list but there were 0 items actually selected, then returning true may be the better option.
        /// </para>
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <returns>Whether the action execution was handled</returns>
        public abstract Task<bool> ExecuteAsync(AnActionEventArgs e);

        /// <summary>
        /// Checks if this action can actually be executed. This typically isn't checked before
        /// <see cref="ExecuteAsync"/> is invoked; this is mainly used by the UI to determine if
        /// something like a button or menu item is actually clickable
        /// <para>
        /// This method should be quick to execute, as it may be called quite often
        /// </para>
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <returns>This action's presentation</returns>
        public virtual bool CanExecute(AnActionEventArgs e) {
            return true;
        }
    }
}