using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FramePFX.Converters {
    public class NullConverter : IValueConverter {
        public object NullValue { get; set; }
        public object NonNullValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value == null ? this.NullValue : this.NonNullValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class NullToVisibilityConverter : NullConverter {
        public new Visibility NullValue {
            get => (Visibility) base.NullValue;
            set => base.NullValue = value;
        }

        public new Visibility NonNullValue {
            get => (Visibility) base.NonNullValue;
            set => base.NonNullValue = value;
        }
    }
}