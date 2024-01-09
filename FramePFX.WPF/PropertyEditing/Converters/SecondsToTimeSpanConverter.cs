using System;
using System.Globalization;
using System.Windows.Data;

namespace FramePFX.WPF.PropertyEditing.Converters {
    public class SecondsToTimeSpanConverter : IValueConverter {
        public static SecondsToTimeSpanConverter Instance { get; } = new SecondsToTimeSpanConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Convert(value, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Convert(value, culture);
        }

        public static object Convert(object value, CultureInfo culture) {
            if (value is TimeSpan span) {
                return span.TotalSeconds;
            }
            else if (value is IConvertible convertible) {
                double seconds = convertible.ToDouble(culture);
                return TimeSpan.FromSeconds(seconds);
            }
            else {
                throw new Exception("Invalid conversion value: " + value);
            }
        }
    }
}