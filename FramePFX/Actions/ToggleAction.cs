using System.Threading.Tasks;

namespace FramePFX.Actions {
    public abstract class ToggleAction : ExecutableAction {
        public const string IsToggledKey = "toggled";

        /// <summary>
        /// Gets whether the given event context is toggled or not
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <returns>A nullable boolean that states the toggle state, or null if no toggle state is present</returns>
        public virtual bool? GetIsToggled(ActionEventArgs e) {
            return e.DataContext.TryGet(IsToggledKey, out bool value) ? (bool?) value : null;
        }

        public override async Task<bool> ExecuteAsync(ActionEventArgs e) {
            bool? result = this.GetIsToggled(e);
            if (result.HasValue) {
                return await this.OnToggled(e, result.Value);
            }
            else {
                return await this.ExecuteNoToggle(e);
            }
        }

        /// <summary>
        /// Called when the action is executed with the given toggle state
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <param name="isToggled">The toggle state of whatever called the action</param>
        /// <returns>Whether the action was executed successfully</returns>
        protected abstract Task<bool> OnToggled(ActionEventArgs e, bool isToggled);

        /// <summary>
        /// Called when the action was executed without any toggle info. This can be
        /// used to, for example, invert a known toggle state
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <returns>Whether the action was executed successfully</returns>
        protected abstract Task<bool> ExecuteNoToggle(ActionEventArgs e);

        public override bool CanExecute(ActionEventArgs e) {
            bool? result = this.GetIsToggled(e);
            return result.HasValue ? this.CanExecute(e, result.Value) : this.CanExecuteNoToggle(e);
        }

        protected virtual bool CanExecute(ActionEventArgs e, bool isToggled) {
            return true;
        }

        protected virtual bool CanExecuteNoToggle(ActionEventArgs e) {
            return this.CanExecute(e, false);
        }
    }
}