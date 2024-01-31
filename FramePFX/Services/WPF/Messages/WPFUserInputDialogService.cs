using System;
using FramePFX.Services.Messages;

namespace FramePFX.Services.WPF.Messages {
    public class WPFUserInputDialogService : IUserInputDialogService {
        public string ShowSingleInputDialog(string caption, string message, string defaultInput = null, Predicate<string> validate = null, bool allowEmptyString = false) {
            SingleInputDialog dialog = new SingleInputDialog() {
                Title = caption ?? "Input value",
                Message = message,
                InputValue = defaultInput,
                Validator = validate,
                IsEmptyStringAllowed = allowEmptyString
            };

            return dialog.ShowDialogAndGetResult(out string output) ? output : null;
        }
    }
}