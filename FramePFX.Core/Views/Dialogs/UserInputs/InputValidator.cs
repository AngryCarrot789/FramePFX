using System;

namespace FramePFX.Core.Views.Dialogs.UserInputs {
    public class InputValidator {
        public Predicate<string> Predicate { get; }
        public string ErrorMessage { get; }

        public InputValidator(Predicate<string> predicate, string errorMessage) {
            this.Predicate = predicate;
            this.ErrorMessage = errorMessage;
        }
    }
}