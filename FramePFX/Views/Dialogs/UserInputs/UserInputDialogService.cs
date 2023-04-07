using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Views.Dialogs.UserInputs {
    public class UserInputDialogService : IUserInputDialogService {
        public string ShowSingleInputDialog(string title = "Input a value", string message = "Input a new valid", string def = null, InputValidator validator = null) {
            SingleUserInputWindow window = new SingleUserInputWindow();
            SingleInputViewModel vm = new SingleInputViewModel(window) {
                Title = title,
                Message = message,
                Input = def
            };

            window.DataContext = vm;
            if (validator != null && window.InputValidationRule != null) {
                window.InputValidationRule.Validator = validator;
            }

            return window.ShowDialog() == true ? (vm.Input ?? "") : null;
        }
    }
}