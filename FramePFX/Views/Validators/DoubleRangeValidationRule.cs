// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Globalization;
using System.Windows.Controls;

namespace FramePFX.Views.Validators {
    public class DoubleRangeValidationRule : ValidationRule {
        public double MinValue { get; set; }
        public double MaxValue { get; set; }

        public string MinValueMessage { get; set; }
        public string MaxValueMessage { get; set; }

        public override ValidationResult Validate(object obj, CultureInfo cultureInfo) {
            double value;
            if (obj is double) {
                value = (double) obj;
            }
            else if (obj is string str) {
                if (string.IsNullOrWhiteSpace(str)) {
                    return new ValidationResult(false, "Value cannot be empty or whitespaces");
                }
                else if (!double.TryParse(str, out value)) {
                    return new ValidationResult(false, "Value is not a decimal number");
                }
            }
            else {
                return new ValidationResult(false, "Invalid input value");
            }

            if (value < this.MinValue) {
                return new ValidationResult(false, this.MinValueMessage == null ? "Value is too small" : string.Format(this.MinValueMessage, value, this.MinValue));
            }
            else if (value > this.MaxValue) {
                return new ValidationResult(false, this.MaxValueMessage == null ? "Value is too big" : string.Format(this.MaxValueMessage, value, this.MaxValue));
            }
            else {
                return ValidationResult.ValidResult;
            }
        }
    }
}