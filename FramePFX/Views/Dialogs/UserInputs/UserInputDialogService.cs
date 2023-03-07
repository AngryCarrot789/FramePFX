using FrameControl.Core.Views.Dialogs.UserInputs;
using FrameControl.Views.Dialogs.FilePicking;

namespace FrameControl.Views.Dialogs.UserInputs {
    public class UserInputDialogService : IUserInputDialogService {
        public string ShowSingleInputDialog(string title = "Input a value", string message = "Input a new valid", string def = null) {
            SingleUserInputWindow window = new SingleUserInputWindow();
            window.Owner = FolderPicker.GetCurrentActiveWindow();
            SingleInputViewModel vm = new SingleInputViewModel(window) {
                Title = title,
                Message = message,
                Input = def
            };

            window.DataContext = vm;
            return window.ShowDialog() == true ? (vm.Input ?? "") : null;
        }
    }
}