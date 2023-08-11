using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FramePFX.Editor.Exporting.Converters {
    public class BitRateToMbConverter : IValueConverter {
        public static BitRateToMbConverter Instance { get; } = new BitRateToMbConverter();

        public int RoundedPlaces { get; set; } = 2;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is long bits) {
                return Math.Round(bits / 1000000d, this.RoundedPlaces);
            }
            else {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is double mbits) {
                return (long) (mbits / 1000000d); // slight data loss due to rounding
            }
            else {
                return DependencyProperty.UnsetValue;
            }
        }
    }
}