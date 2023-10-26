using ColorPicker.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ColorPicker.Converters {
    [ValueConversion(typeof(double), typeof(string))]
    internal class RangeConstrainedDoubleToDoubleConverter : DependencyObject, IValueConverter {
        public static readonly DependencyProperty MinProperty = DependencyProperty.Register(nameof(Min), typeof(double), typeof(RangeConstrainedDoubleToDoubleConverter), new PropertyMetadata(0.0));

        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register(nameof(Max), typeof(double), typeof(RangeConstrainedDoubleToDoubleConverter), new PropertyMetadata(1.0));

        public double Min {
            get => (double) this.GetValue(MinProperty);
            set => this.SetValue(MinProperty, value);
        }

        public double Max {
            get => (double) this.GetValue(MaxProperty);
            set => this.SetValue(MaxProperty, value);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!double.TryParse(((string) value).Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                return DependencyProperty.UnsetValue;
            return MathHelper.Clamp(result, this.Min, this.Max);
        }
    }
}