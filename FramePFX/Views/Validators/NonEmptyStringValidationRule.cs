using System.Globalization;
using System.Windows.Controls;

namespace FramePFX.Views.Validators {
    public class NonEmptyStringValidationRule : ValidationRule {
        public static NonEmptyStringValidationRule Default = new NonEmptyStringValidationRule() {
            NullOrEmptyMessage = "Value cannot be an empty string"
        };

        public object NullOrEmptyMessage { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if (value == null || value is string str && str.Length < 1) {
                return new ValidationResult(false, this.NullOrEmptyMessage);
            }

            return ValidationResult.ValidResult;
        }
    }
}