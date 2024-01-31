using System;

namespace FramePFX.Services.Messages {
    public interface IUserInputDialogService {
        /// <summary>
        /// Shows a dialog which accepts a general text input, optionally with a validation predicate which
        /// prevents the dialog closing successfully if the value fails the validation
        /// </summary>
        /// <param name="caption">The window titlebar</param>
        /// <param name="message">A message to present to the user above the text input</param>
        /// <param name="defaultInput">
        /// The text that is placed in the text input by default. Default is null aka empty string
        /// </param>
        /// <param name="validate">
        /// A validator predicate. Default is null, meaning any value is allowed.
        /// This predicate should be fast, as it will be executed whenever the user types something
        /// </param>
        /// <param name="allowEmptyString">
        /// Allows this method to return an empty string if the text input is empty. Default is false
        /// </param>
        /// <returns>The text in the input area, or null if the input was empty</returns>
        string ShowSingleInputDialog(string caption, string message, string defaultInput = null, Predicate<string> validate = null, bool allowEmptyString = false);
    }
}