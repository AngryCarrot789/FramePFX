using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Views.Dialogs.UserInputs {
    public class UserInputDialogService : IUserInputDialogService {
        public string ShowSingleInputDialog(string title = "Input a value", string message = "Input a new valid", string def = null, InputValidator validator = null) {
            SingleInputViewModel vm = new SingleInputViewModel() {
                Title = title,
                Message = message,
                Input = def,
                ValidateInput = validator
            };

            return this.ShowSingleInputDialog(vm) ? vm.Input ?? "" : null;
        }

        public bool ShowSingleInputDialog(SingleInputViewModel viewModel) {
            SingleUserInputWindow window = new SingleUserInputWindow {
                DataContext = viewModel
            };

            viewModel.Dialog = window;
            if (viewModel.ValidateInput != null && window.InputValidationRule != null) {
                window.InputValidationRule.Validator = viewModel.ValidateInput;
            }

            return window.ShowDialog() == true;
        }

        public bool ShowDoubleInputDialog(DoubleInputViewModel viewModel) {
            DoubleUserInputWindow window = new DoubleUserInputWindow {
                DataContext = viewModel
            };

            viewModel.Dialog = window;
            if (viewModel.ValidateInputA != null && window.InputValidationRuleA != null)
                window.InputValidationRuleA.Validator = viewModel.ValidateInputA;
            if (viewModel.ValidateInputB != null && window.InputValidationRuleB != null)
                window.InputValidationRuleB.Validator = viewModel.ValidateInputB;
            return window.ShowDialog() == true;
        }
    }
}