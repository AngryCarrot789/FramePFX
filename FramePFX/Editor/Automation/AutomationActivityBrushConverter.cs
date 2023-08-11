using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FramePFX.Editor.Automation {
    public class AutomationActivityBrushConverter : IMultiValueConverter {
        public Brush ActiveBrush { get; set; } = Brushes.OrangeRed;

        public Brush ForcedActive { get; set; } = Brushes.DodgerBlue;

        public Brush InactiveBrush { get; set; } = Brushes.DarkGray;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || values.Length != 2 || values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue) {
                return DependencyProperty.UnsetValue;
            }

            bool isInUse = (bool) values[0];
            bool selected = (bool) values[1];
            if (isInUse) {
                return this.ActiveBrush; //selected ? this.ActiveBrush : this.ForcedActive;
            }
            else {
                return this.InactiveBrush;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}