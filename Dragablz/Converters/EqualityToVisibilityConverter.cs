using System;
using System.Globalization;
using System.Windows.Data;

namespace Dragablz.Converters {
    public class EqualityToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Equals(value, parameter) ? BooleanAndToVisibilityConverter.VisibleBox : BooleanAndToVisibilityConverter.CollapsedBox;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}