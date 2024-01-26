using System.Threading.Tasks;
using FramePFX.Interactivity;

namespace FramePFX.Actions {
    /// <summary>
    /// Represents some sort of action that can be executed. This class is designed to be used as a singleton,
    /// meaning there should only ever be a single instance of any implementation of this class
    /// <para>
    /// These actions are only really used by shortcut processors due to the lack of explicit context during key strokes,
    /// therefore, the context can be calculated in an action and the appropriate methods can be invoked (e.g. save project
    /// by finding a video editor or something that has access to the video editor such as a clip or timeline).
    /// <para>
    /// Actions are also used by context menus because of a different reason (creating duplicate context menu items for UI
    /// objects in a list for example is just very wasteful, even if it uses very little memory. Therefore, these actions allow
    /// dynamically created/removed context menu items that execute these actions)
    /// </para>
    /// </para>
    /// <para>
    /// These actions can be executed through the <see cref="ActionManager.Execute(string, IDataContext, bool)"/> function
    /// </para>
    /// </summary>
    public abstract class AnAction {
        /// <summary>
        /// Gets the unique singleton ID for this context action. This is set after the
        /// current instance is registered with a <see cref="ActionManager"/>
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Gets or sets the display name for this action. May be null, making it an unnamed action
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a readable description for what this action
        /// </summary>
        public string Description { get; set; }

        protected AnAction() {
        }

        /// <summary>
        /// Checks if this action can actually be executed. This typically isn't checked before
        /// <see cref="ExecuteAsync"/> is invoked; this is mainly used by the UI to determine if
        /// something like a button or menu item is actually clickable
        /// <para>
        /// This method should be quick to execute, as it may be called quite often
        /// </para>
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <returns>True if executing this action would most likely result in success, otherwise false</returns>
        public virtual bool CanExecute(AnActionEventArgs e) {
            return true;
        }

        /// <summary>
        /// Executes this specific action with the given action event args
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <returns>Whether the action execution was handled</returns>
        public abstract Task ExecuteAsync(AnActionEventArgs e);
    }
}