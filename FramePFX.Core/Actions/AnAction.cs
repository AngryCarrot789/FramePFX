using System;
using System.Threading.Tasks;

namespace SharpPadV2.Core.Actions {
    /// <summary>
    /// Represents some sort of action that can be executed. This class is designed to be used as a singleton,
    /// meaning there should only ever be a single instance of any implementation of this class
    /// <para>
    /// These actions can be executed through the <see cref="ActionManager.Execute(string, Contexts.IDataContext, bool)"/> function
    /// </para>
    /// </summary>
    public abstract class AnAction {
        private static readonly Func<string> ProvideNullString = () => null;

        public Func<string> Header { get; }

        public Func<string> Description { get; }

        protected AnAction(Func<string> header, Func<string> description) {
            this.Header = header ?? ProvideNullString;
            this.Description = description ?? ProvideNullString;
        }

        /// <summary>
        /// Executes this specific action with the given action event args
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <returns>Whether the action was executed successfully</returns>
        public abstract Task<bool> ExecuteAsync(AnActionEventArgs e);

        /// <summary>
        /// Gets this action's presentation. This is used by the UI to determine how to present the action. For example, this may return a visible
        /// but disabled presentation, meaning a context menu item or button (that fires this action) is visible, but cannot be clicked
        /// <para>
        /// This method should be quick to execute, as it may be called quite often
        /// </para>
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <returns>This action's presentation</returns>
        public virtual Presentation GetPresentation(AnActionEventArgs e) {
            return Presentation.VisibleAndEnabled;
        }
    }
}