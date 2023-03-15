using System.Globalization;
using System.Windows.Controls;
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Views.Dialogs.UserInputs {
    public class SingleInputValidationRule : ValidationRule {
        public InputValidator Validator { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            return this.Validator != null && this.Validator.Predicate(value?.ToString()) ? new ValidationResult(false, this.Validator.ErrorMessage ?? "Invalid input") : ValidationResult.ValidResult;
        }
    }
}