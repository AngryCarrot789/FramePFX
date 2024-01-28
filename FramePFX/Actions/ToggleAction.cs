using System.Threading.Tasks;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Actions {
    public abstract class ToggleAction : AnAction {
        public static readonly DataKey<bool> IsToggledKey = new DataKey<bool>("Toggled");

        /// <summary>
        /// Gets whether the given event context is toggled or not
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <returns>A nullable boolean that states the toggle state, or null if no toggle state is present</returns>
        public virtual bool? GetIsToggled(AnActionEventArgs e) {
            return e.DataContext.TryGetContext(IsToggledKey, out bool value) ? (bool?) value : null;
        }

        public override Task ExecuteAsync(AnActionEventArgs e) {
            bool? result = this.GetIsToggled(e);
            if (result.HasValue) {
                return this.OnToggled(e, result.Value);
            }
            else {
                return this.ExecuteNoToggle(e);
            }
        }

        /// <summary>
        /// Called when the action is executed with the given toggle state
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <param name="isToggled">The toggle state of whatever called the action</param>
        /// <returns>Whether the action was executed successfully</returns>
        protected abstract Task<bool> OnToggled(AnActionEventArgs e, bool isToggled);

        /// <summary>
        /// Called when the action was executed without any toggle info. This can be
        /// used to, for example, invert a known toggle state
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <returns>Whether the action was executed successfully</returns>
        protected abstract Task<bool> ExecuteNoToggle(AnActionEventArgs e);

        public override bool CanExecute(AnActionEventArgs e) {
            bool? result = this.GetIsToggled(e);
            return result.HasValue ? this.CanExecute(e, result.Value) : this.CanExecuteNoToggle(e);
        }

        protected virtual bool CanExecute(AnActionEventArgs e, bool isToggled) {
            return true;
        }

        protected virtual bool CanExecuteNoToggle(AnActionEventArgs e) {
            return this.CanExecute(e, false);
        }
    }
}