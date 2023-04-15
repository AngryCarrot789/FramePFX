namespace SharpPadV2.Core.Views.Dialogs.UserInputs {
    public interface IUserInputDialogService {
        string ShowSingleInputDialog(string title = "Input a value", string message = "Input a new valid", string def = null, InputValidator validator = null);
        bool ShowSingleInputDialog(SingleInputViewModel viewModel);
        bool ShowDoubleInputDialog(DoubleInputViewModel viewModel);
    }
}