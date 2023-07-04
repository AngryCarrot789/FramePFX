using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Dragablz.Converters {
    public class BooleanAndToVisibilityConverter : IMultiValueConverter {
        public static readonly object VisibleBox = Visibility.Visible;
        public static readonly object CollapsedBox = Visibility.Collapsed;
        private static readonly Func<object, bool> GetBool = v => v is bool b && b;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null) {
                return CollapsedBox;
            }

            return values.Select(GetBool).All(b => b) ? VisibleBox : CollapsedBox;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            return null;
        }
    }
}