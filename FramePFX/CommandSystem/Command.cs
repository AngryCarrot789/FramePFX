using FramePFX.Interactivity.DataContexts;

namespace FramePFX.CommandSystem {
    /// <summary>
    /// Represents some sort of action that can be executed. Commands use provided contextual
    /// information (see <see cref="CommandEventArgs.DataContext"/>) to do work. Commands do
    /// their work in the <see cref="Execute"/> method, and can optionally specify their
    /// executability via the <see cref="CanExecute"/> method
    /// <para>
    /// Commands are the primary things used by the shortcut system to do some work. They
    /// can also be used by things like context menus
    /// </para>
    /// <para>
    /// These commands can be executed through the <see cref="CommandManager.Execute(string, Command, IDataContext, bool)"/> function
    /// </para>
    /// </summary>
    public abstract class Command {
        protected Command() {
        }

        /// <summary>
        /// Checks if this command can actually be executed. This typically isn't checked before
        /// <see cref="Execute"/> is invoked; this is mainly used by the UI to determine if
        /// something like a button or menu item is actually clickable
        /// <para>
        /// This method should be quick to execute, as it may be called quite often
        /// </para>
        /// </summary>
        /// <param name="e">The command event args, containing info about the current context</param>
        /// <returns>
        /// True if executing this command would most likely result in success, otherwise false
        /// </returns>
        public virtual bool CanExecute(CommandEventArgs e) {
            return true;
        }

        /// <summary>
        /// Executes this specific command with the given command event args
        /// </summary>
        /// <param name="e">The command event args, containing info about the current context</param>
        public abstract void Execute(CommandEventArgs e);
    }
}