using FramePFX.Interactivity.DataContexts;

namespace FramePFX.CommandSystem {
    public abstract class ToggleBasedCommand : Command {
        public static readonly DataKey<bool> IsToggledKey = DataKey<bool>.Create("Toggled");

        /// <summary>
        /// Gets whether the given event context is toggled or not
        /// </summary>
        /// <param name="e">The command event args, containing info about the current context</param>
        /// <returns>A nullable boolean that states the toggle state, or null if no toggle state is present</returns>
        public virtual bool? GetIsToggled(CommandEventArgs e) {
            return IsToggledKey.TryGetContext(e.DataContext, out bool value) ? (bool?) value : null;
        }

        public override void Execute(CommandEventArgs e) {
            bool? result = this.GetIsToggled(e);
            if (result.HasValue) {
                this.OnToggled(e, result.Value);
            }
            else {
                this.ExecuteNoToggle(e);
            }
        }

        /// <summary>
        /// Called when the command is executed with the given toggle state
        /// </summary>
        /// <param name="e">The command event args, containing info about the current context</param>
        /// <param name="isToggled">The toggle state of whatever called the command</param>
        /// <returns>Whether the command was executed successfully</returns>
        protected abstract void OnToggled(CommandEventArgs e, bool isToggled);

        /// <summary>
        /// Called when the command was executed without any toggle info. This can be
        /// used to, for example, invert a known toggle state
        /// </summary>
        /// <param name="e">The command event args, containing info about the current context</param>
        /// <returns>Whether the command was executed successfully</returns>
        protected abstract void ExecuteNoToggle(CommandEventArgs e);

        public override bool CanExecute(CommandEventArgs e) {
            bool? result = this.GetIsToggled(e);
            return result.HasValue ? this.CanExecute(e, result.Value) : this.CanExecuteNoToggle(e);
        }

        protected virtual bool CanExecute(CommandEventArgs e, bool isToggled) {
            return true;
        }

        protected virtual bool CanExecuteNoToggle(CommandEventArgs e) {
            return this.CanExecute(e, false);
        }
    }
}