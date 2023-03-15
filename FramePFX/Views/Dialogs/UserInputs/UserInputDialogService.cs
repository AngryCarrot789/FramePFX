using FramePFX.Core.Views.Dialogs.UserInputs;
using FramePFX.Views.Dialogs.FilePicking;

namespace FramePFX.Views.Dialogs.UserInputs {
    public class UserInputDialogService : IUserInputDialogService {
        public string ShowSingleInputDialog(string title = "Input a value", string message = "Input a new valid", string def = null, InputValidator validator = null) {
            SingleUserInputWindow window = new SingleUserInputWindow();
            window.Owner = FolderPicker.GetCurrentActiveWindow();
            SingleInputViewModel vm = new SingleInputViewModel(window) {
                Title = title,
                Message = message,
                Input = def
            };

            window.DataContext = vm;
            window.InputValidationRule.Validator = validator;
            return window.ShowDialog() == true ? (vm.Input ?? "") : null;
        }
    }
}