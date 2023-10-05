using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SkiaSharp;

namespace FramePFX.WPF.Converters {
    public class SKColorToBrushConverter : IValueConverter {
        public static SKColorToBrushConverter Instance { get; } = new SKColorToBrushConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is SKColor colour) {
                return new SolidColorBrush(Color.FromArgb(colour.Alpha, colour.Red, colour.Green, colour.Blue));
            }
            else if (value is SKColorF colourF) {
                return new SolidColorBrush(Color.FromScRgb(colourF.Alpha, colourF.Red, colourF.Green, colourF.Blue));
            }
            else if (value == null || value == DependencyProperty.UnsetValue) {
                return DependencyProperty.UnsetValue;
            }
            else {
                throw new Exception("Invalid value: " + value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}