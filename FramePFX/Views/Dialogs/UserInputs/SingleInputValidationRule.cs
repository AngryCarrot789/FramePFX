using System.Globalization;
using System.Windows.Controls;
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Views.Dialogs.UserInputs {
    public class SingleInputValidationRule : ValidationRule {
        public InputValidator Validator { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if (this.Validator != null && this.Validator.InvalidationChecker(value?.ToString(), out string msg)) {
                return new ValidationResult(false, msg ?? "Invalid input");
            }
            else {
                return ValidationResult.ValidResult;
            }
        }
    }
}