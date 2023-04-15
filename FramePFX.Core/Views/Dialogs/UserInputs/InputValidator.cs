using System;

namespace FramePFX.Core.Views.Dialogs.UserInputs {
    public class InputValidator {
        public delegate bool IsInputInvalidDelegate(string input, out string errorMessage);

        /// <summary>
        /// A predicate to check if the input is invalid or not. If this returns True, then the input is invalid and an error message may be available
        /// </summary>
        public IsInputInvalidDelegate InvalidationChecker { get; }

        /// <summary>
        /// Creates a new input validator
        /// </summary>
        /// <param name="invalidationChecker">A predicate to check if the input is valid or not. True = value, False = invalid (and error message is displayed)</param>
        /// <param name="errorMessage">The error message to display</param>
        public InputValidator(IsInputInvalidDelegate invalidationChecker) {
            this.InvalidationChecker = invalidationChecker;
        }

        public static InputValidator SingleError(Predicate<string> isInvalidCallback, string errorMessage) {
            return new InputValidator((string input, out string msg) => {
                if (isInvalidCallback(input)) {
                    msg = errorMessage;
                    return true;
                }
                else {
                    msg = null;
                    return false;
                }
            });
        }

        public static InputValidator FromFunc(Func<string, string> inputToError) {
            return new InputValidator((string input, out string msg) => (msg = inputToError(input)) != null);
        }
    }

    public static class Validators {
        public static InputValidator ForNonEmptyString(string nullMessage) {
            return InputValidator.FromFunc((x) => string.IsNullOrEmpty(x) ? (nullMessage ?? "Input value cannot be null") : null);
        }
    }
}