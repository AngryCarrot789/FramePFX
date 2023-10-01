using System.Globalization;
using System.Windows.Controls;
using SkiaSharp;

namespace FramePFX.WPF.Validators
{
    public class FontFamilyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string input)
            {
                SKTypeface result = SKTypeface.FromFamilyName(input);
                if (result == null)
                {
                    return new ValidationResult(false, "Unknown font: " + input);
                }

                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, "Invalid input");
            }
        }
    }
}