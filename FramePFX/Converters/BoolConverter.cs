using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FramePFX.Converters {
    public class BoolConverter : IValueConverter {
        public object TrueValue { get; set; }

        public object FalseValue { get; set; }

        public object UnsetValue { get; set; }

        public object NonBoolValue { get; set; }

        public bool ThrowForUnset { get; set; }

        public bool ThrowForNonBool { get; set; }

        public BoolConverter() {
            this.UnsetValue = DependencyProperty.UnsetValue;
            this.NonBoolValue = DependencyProperty.UnsetValue;
            this.ThrowForUnset = false;
            this.ThrowForNonBool = false;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool boolean) {
                return boolean ? this.TrueValue : this.FalseValue;
            }
            else if (value == DependencyProperty.UnsetValue) {
                return this.ThrowForUnset ? throw new Exception("Unset value not allowed") : this.UnsetValue;
            }
            else if (this.ThrowForNonBool) {
                throw new Exception("Expected boolean, got " + value);
            }
            else {
                return this.NonBoolValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == this.TrueValue) {
                return true;
            }
            else if (value == this.FalseValue) {
                return false;
            }
            else if (value == this.UnsetValue) {
                return this.ThrowForUnset ? throw new Exception("Unset value not allowed") : DependencyProperty.UnsetValue;
            }
            else if (this.ThrowForNonBool) {
                throw new Exception("Expected boolean, got " + value);
            }
            else {
                throw new Exception("Cannot convert back from " + value);
            }
        }
    }

    public class InvertBoolConverter : BoolConverter {
        public InvertBoolConverter() {
            this.TrueValue = false;
            this.FalseValue = true;
        }
    }

    public class BoolToVisibilityConverter : BoolConverter {
        public new Visibility TrueValue {
            get => (Visibility) base.TrueValue;
            set => base.TrueValue = value;
        }

        public new Visibility FalseValue {
            get => (Visibility) base.FalseValue;
            set => base.FalseValue = value;
        }

        public BoolToVisibilityConverter() {

        }
    }
}