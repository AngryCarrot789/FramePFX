using System.Threading.Tasks;

namespace FrameControlEx.Core.Views.Dialogs.UserInputs {
    public interface IUserInputDialogService {
        // TODO: Convert to async
        Task<string> ShowSingleInputDialogAsync(string title = "Input a value", string message = "Input a new valid", string def = null, InputValidator validator = null);
        Task<bool> ShowSingleInputDialogAsync(SingleInputViewModel viewModel);
        Task<bool> ShowDoubleInputDialogAsync(DoubleInputViewModel viewModel);
    }
}