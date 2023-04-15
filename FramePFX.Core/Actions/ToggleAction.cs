using System;
using System.Threading.Tasks;

namespace SharpPadV2.Core.Actions {
    public abstract class ToggleAction : AnAction {
        public const string IsToggledKey = "toggled";

        protected ToggleAction(Func<string> header, Func<string> description) : base(header, description) {

        }

        /// <summary>
        /// Asynchronously gets whether the current context is toggled or not
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <returns>A nullable boolean that states the toggle state, or null if no toggle state is present</returns>
        public virtual Task<bool?> GetIsToggledAsync(AnActionEventArgs e) {
            return Task.FromResult(this.GetIsToggled(e));
        }

        /// <summary>
        /// Synchronously gets whether the current context is toggled or not. This is mainly used to
        /// get the presentation of this toggle action, and is generally not directly called by <see cref="ExecuteAsync"/>
        /// (however by default, <see cref="GetIsToggledAsync"/> will call this method which is called by <see cref="ExecuteAsync"/>)
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <returns>A nullable boolean that states the toggle state, or null if no toggle state is present</returns>
        public virtual bool? GetIsToggled(AnActionEventArgs e) {
            return e.DataContext.TryGet(IsToggledKey, out bool value) ? (bool?) value : null;
        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            bool? result = await this.GetIsToggledAsync(e);
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
        public abstract Task<bool> OnToggled(AnActionEventArgs e, bool isToggled);

        /// <summary>
        /// Called when the action was executed without any toggle info. This can be
        /// used to, for example, invert a known toggle state
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <returns>Whether the action was executed successfully</returns>
        public abstract Task<bool> ExecuteNoToggle(AnActionEventArgs e);

        public override Presentation GetPresentation(AnActionEventArgs e) {
            bool? result = this.GetIsToggled(e);
            if (result.HasValue) {
                return this.GetPresentation(e, result.Value);
            }
            else {
                return this.GetPresentationNoToggle(e);
            }
        }

        public virtual Presentation GetPresentation(AnActionEventArgs e, bool isToggled) {
            return isToggled ? Presentation.VisibleAndEnabled : Presentation.VisibleAndDisabled;
        }

        public virtual Presentation GetPresentationNoToggle(AnActionEventArgs e) {
            return this.GetPresentation(e, false);
        }
    }
}