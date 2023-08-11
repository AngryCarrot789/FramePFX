using System;
using System.Globalization;
using System.Windows.Data;

namespace FramePFX.Converters {
    public class FloatToDoubleConverter : IValueConverter {
        public static FloatToDoubleConverter Instance { get; } = new FloatToDoubleConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            switch (value) {
                case float f: return (double) f;
                case double _: return value;
                default: return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            switch (value) {
                case double d: return (float) d;
                case float _: return value;
                default: return value;
            }
        }
    }
}