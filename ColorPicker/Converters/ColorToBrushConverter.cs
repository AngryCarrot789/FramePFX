using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ColorPicker.Converters {
    [ValueConversion(typeof(Color), typeof(SolidColorBrush))]
    internal class ColorToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || value == DependencyProperty.UnsetValue)
                return DependencyProperty.UnsetValue;
            Color col = (Color) value;
            Color c = Color.FromArgb(col.A, col.R, col.G, col.B);
            return new SolidColorBrush(c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || value == DependencyProperty.UnsetValue)
                return DependencyProperty.UnsetValue;
            SolidColorBrush c = (SolidColorBrush) value;
            Color col = Color.FromArgb(c.Color.A, c.Color.R, c.Color.G, c.Color.B);
            return col;
        }
    }
}