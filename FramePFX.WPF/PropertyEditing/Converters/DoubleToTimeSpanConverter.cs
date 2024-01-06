using System;
using System.Globalization;
using System.Windows.Data;

namespace FramePFX.WPF.PropertyEditing.Converters {
    public class DoubleToTimeSpanConverter : IValueConverter {
        public static DoubleToTimeSpanConverter Instance { get; } = new DoubleToTimeSpanConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Convert(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Convert(value);
        }

        public static object Convert(object value) {
            if (value is double d) {
                return TimeSpan.FromSeconds(d);
            }
            else if (value is TimeSpan span) {
                return span.TotalSeconds;
            }
            else {
                throw new Exception("Invalid conversion value: " + value);
            }
        }
    }
}