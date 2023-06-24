using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FramePFX.Editor.Automation {
    public class AutomationActivityBrushConverter : IValueConverter {
        public Brush ActiveBrush { get; set; } = Brushes.OrangeRed;

        public Brush InactiveBrush { get; set; } = Brushes.DarkGray;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool b) {
                return b ? this.ActiveBrush : this.InactiveBrush;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}