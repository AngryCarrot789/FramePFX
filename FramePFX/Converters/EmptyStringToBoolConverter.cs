using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FrameControlEx.Converters {
    public class EmptyStringToBoolConverter : IValueConverter {
        public object NullValue { get; set; }

        public object EmptyValue { get; set; }

        public object NonEmptyValue { get; set; }

        public object NonStringValue { get; set; }
        public object UnsetValue { get; set; }

        public bool ThrowForUnset { get; set; }
        public bool ThrowForNonString { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string str) {
                return str.Length < 1 ? this.EmptyValue : this.NonEmptyValue;
            }
            else if (value == DependencyProperty.UnsetValue) {
                return this.ThrowForUnset ? throw new Exception("Unset value not allowed") : this.UnsetValue;
            }
            else if (value == null) {
                return this.NullValue;
            }
            else if (this.ThrowForNonString) {
                throw new Exception("Expected string, got " + value);
            }
            else {
                return this.NonStringValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public EmptyStringToBoolConverter() {
            this.UnsetValue = DependencyProperty.UnsetValue;
            this.NonStringValue = DependencyProperty.UnsetValue;
        }
    }

    public class EmptyStringToVisibilityConverter : EmptyStringToBoolConverter {
        public new Visibility NullValue {
            get => (Visibility) base.NullValue;
            set => base.NullValue = NullToVisibilityConverter.Box(value);
        }

        public new Visibility EmptyValue {
            get => (Visibility) base.EmptyValue;
            set => base.EmptyValue = NullToVisibilityConverter.Box(value);
        }

        public new Visibility NonEmptyValue {
            get => (Visibility) base.NonEmptyValue;
            set => base.NonEmptyValue = NullToVisibilityConverter.Box(value);
        }

        public new Visibility NonStringValue {
            get => (Visibility) base.NonStringValue;
            set => base.NonStringValue = NullToVisibilityConverter.Box(value);
        }

        public new Visibility UnsetValue {
            get => (Visibility) base.UnsetValue;
            set => base.UnsetValue = NullToVisibilityConverter.Box(value);
        }

        public EmptyStringToVisibilityConverter() {
            base.NullValue = NullToVisibilityConverter.CollapsedBox;
            base.EmptyValue = NullToVisibilityConverter.CollapsedBox;
            base.NonEmptyValue = NullToVisibilityConverter.VisibleBox;
        }
    }
}