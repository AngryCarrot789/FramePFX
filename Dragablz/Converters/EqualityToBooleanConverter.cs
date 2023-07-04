using System;
using System.Globalization;
using System.Windows.Data;

namespace Dragablz.Converters {
    public class EqualityToBooleanConverter : IValueConverter {
        private static readonly object TrueBox = true;
        private static readonly object FalseBox = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Equals(value, parameter) ? TrueBox : FalseBox;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}