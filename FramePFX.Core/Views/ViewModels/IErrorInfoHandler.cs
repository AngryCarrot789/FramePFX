using System.Collections.Generic;

namespace FramePFX.Core.Views.ViewModels {
    /// <summary>
    /// An interface added to a view model to indicate that it wants to listen for when errors in the view have changed
    /// </summary>
    public interface IErrorInfoHandler {
        /// <summary>
        /// Called when errors have been added/removed/changed. This is
        /// useful to, for example, disable the confirm command on a dialog
        /// </summary>
        /// <param name="errors">The errors present. May be empty but will not be null</param>
        void OnErrorsUpdated(Dictionary<string, object> errors);
    }
}