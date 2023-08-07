using System.Globalization;
using System.Windows.Controls;

namespace FramePFX.Views.Validators
{
    public class DoubleRangeValidationRule : ValidationRule
    {
        public double MinValue { get; set; }
        public double MaxValue { get; set; }

        public string MinValueMessage { get; set; }
        public string MaxValueMessage { get; set; }

        public override ValidationResult Validate(object obj, CultureInfo cultureInfo)
        {
            double value;
            if (obj is double)
            {
                value = (double) obj;
            }
            else if (obj is string str)
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return new ValidationResult(false, "Value cannot be empty or whitespaces");
                }
                else if (!double.TryParse(str, out value))
                {
                    return new ValidationResult(false, "Value is not a decimal number");
                }
            }
            else
            {
                return new ValidationResult(false, "Invalid input value");
            }

            if (value < this.MinValue)
            {
                return new ValidationResult(false, this.MinValueMessage == null ? "Value is too small" : string.Format(this.MinValueMessage, value, this.MinValue));
            }
            else if (value > this.MaxValue)
            {
                return new ValidationResult(false, this.MaxValueMessage == null ? "Value is too big" : string.Format(this.MaxValueMessage, value, this.MaxValue));
            }
            else
            {
                return ValidationResult.ValidResult;
            }
        }
    }
}