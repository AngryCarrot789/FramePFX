using System;

namespace FramePFX.Core.Views.Dialogs.UserInputs
{
    /// <summary>
    /// A class used for validating user input
    /// </summary>
    public class InputValidator
    {
        public delegate bool IsInputInvalidDelegate(string input, out string errorMessage);

        /// <summary>
        /// A predicate to check if the input is invalid or not. If this returns True, then the input is invalid and an error message may be available
        /// </summary>
        public IsInputInvalidDelegate IsInvalidFunc { get; }

        /// <summary>
        /// Creates a new input validator
        /// </summary>
        /// <param name="isInvalidFunc">A predicate to check if the input is valid or not. True = value, False = invalid (and error message is displayed)</param>
        /// <param name="errorMessage">The error message to display</param>
        public InputValidator(IsInputInvalidDelegate isInvalidFunc)
        {
            this.IsInvalidFunc = isInvalidFunc;
        }

        public static InputValidator SingleError(Predicate<string> isInvalidCallback, string errorMessage)
        {
            return new InputValidator((string input, out string msg) =>
            {
                if (isInvalidCallback(input))
                {
                    msg = errorMessage;
                    return true;
                }
                else
                {
                    msg = null;
                    return false;
                }
            });
        }

        public static InputValidator FromFunc(Func<string, string> inputToError)
        {
            return new InputValidator((string input, out string msg) => (msg = inputToError(input)) != null);
        }
    }

    public static class Validators
    {
        public static InputValidator ForNonEmptyString(string nullOrEmptyMessage)
        {
            return InputValidator.FromFunc((x) => string.IsNullOrEmpty(x) ? (nullOrEmptyMessage ?? "Input value cannot be an empty string") : null);
        }

        public static InputValidator ForNonWhiteSpaceString(string nullOrEmptyMessage = null)
        {
            return InputValidator.FromFunc((x) =>
            {
                if (string.IsNullOrEmpty(x))
                    return nullOrEmptyMessage ?? "Value cannot be an empty string";
                if (string.IsNullOrWhiteSpace(x))
                    return nullOrEmptyMessage ?? "Value cannot consist of only whitespaces";
                return null;
            });
        }
    }
}